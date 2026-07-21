using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using API.Application.DTO;
using API.Application.Repository.IRepository;
using API.Application.Services;
using API.Application.Services.IServices;
using API.Constantes;
using API.Domain.Entities;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SYSGES_MAGs.Helpers
{
    public class MagProcessingHelper
    {
        public readonly ILogger<MagProcessingService> _logger;
        public readonly IComptesDebiteRedevCarteRepository _comptesDebiteRedevCarteRepository;
        public readonly ITypeMagRepository _typeMagRepository;
        public readonly IComptesOuvertService _comptesOuvertService;

        /// <summary>
        /// numéro d'evenement (valeur incrementielle)
        /// </summary>
        public int _sequence = 52292;

        /// <summary>
        /// limite maximale du numéro d'évènement
        /// </summary>
        public const int MAX_SEQUENCE = 999999;

        /// <summary>
        /// fonction qui effectue le processus de calcul du MAG (Manque à gagner)
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="comptesDebiteRedevCarteRepository"></param>
        /// <param name="typeMagRepository"></param>
        /// <param name="comptesOuvertService"></param>
        public MagProcessingHelper() { }

        public MagProcessingHelper(
            ILogger<MagProcessingService> logger,
            IComptesDebiteRedevCarteRepository comptesDebiteRedevCarteRepository,
            ITypeMagRepository typeMagRepository,
            IComptesOuvertService comptesOuvertService
        )
        {
            _logger = logger;
            _comptesDebiteRedevCarteRepository = comptesDebiteRedevCarteRepository;
            _typeMagRepository = typeMagRepository;
            _comptesOuvertService = comptesOuvertService;
        }

        public async Task<Dictionary<string, List<Apprint>>> callFuncAsync(
            List<ComptesOuvert> comptes,
            StreamReader reader,
            long ligne,
            Dictionary<string, ComptesCtxResponse> comptesCtx,
            List<ComptesOuvert> comptesOuverts,
            Dictionary<string, List<Apprint>> clientPlusUneCarte,
            List<CompteDebiteRedevCarte> comptesDebite
        )
        {
            await _comptesOuvertService.SaveComptesOuvertAsync(comptes);

            await _comptesDebiteRedevCarteRepository.SaveComptesDebiteAsync(comptesDebite);

            return await GetCartesClient(
                reader,
                ligne,
                comptesCtx,
                comptesOuverts,
                clientPlusUneCarte
            );
        }

        public async Task<Dictionary<string, List<Apprint>>> GetCartesClient(
            StreamReader reader,
            long ligne,
            Dictionary<string, ComptesCtxResponse> comptesCtx,
            List<ComptesOuvert> comptesOuverts,
            Dictionary<string, List<Apprint>> clientPlusUneCarte
        )
        {
            string? ligneApprint;

            // Préparation des recherches rapides
            var comptesOuvertsSet = comptesOuverts.Select(x => x.Ncp).ToHashSet();

            var comptesCtxSet = comptesCtx.Keys.ToHashSet();

            var cartesExcluesSet = CardConstants.cartesExclu.ToHashSet();

            var cartesGratuitesSet = CardConstants.cartesGratuite.ToHashSet();

            while ((ligneApprint = await reader.ReadLineAsync()) != null)
            {
                ligne++;

                try
                {
                    if (
                        string.IsNullOrWhiteSpace(ligneApprint)
                        || ligneApprint.TrimStart().StartsWith("99 ")
                    )
                    {
                        continue;
                    }

                    var apprint = ConvertTxtToApprint(ligneApprint, ligne);

                    if (string.IsNullOrWhiteSpace(apprint.DateValiditeAgenceCodeDeviseNumeroCompte))
                    {
                        continue;
                    }

                    // Extraction compte
                    var numeroCompte = apprint.DateValiditeAgenceCodeDeviseNumeroCompte.Substring(
                        12
                    );

                    // Date validité carte
                    var dateValiditeCarte = GetDateValiditeCarte(apprint);

                    bool carteExpiree = dateValiditeCarte <= DateTimeOffset.UtcNow;

                    // Exclusions
                    if (
                        carteExpiree
                        || comptesCtxSet.Contains(numeroCompte)
                        || !comptesOuvertsSet.Contains(numeroCompte)
                        || cartesExcluesSet.Contains(apprint.CodeCarte!)
                        || (!numeroCompte.StartsWith("02") && !numeroCompte.StartsWith("31"))
                    )
                    {
                        //_logger.LogInformation($"Ligne {ligne} : Carte invalide" + apprint);
                        continue;
                    }

                    // Code tarification
                    var codeTarif = apprint.EstActifCodeTarifNumeroCompte?.Substring(1, 2);

                    if (string.IsNullOrEmpty(codeTarif))
                    {
                        continue;
                    }

                    // Carte gratuite uniquement
                    if (!cartesGratuitesSet.Contains(codeTarif))
                    {
                        continue;
                    }

                    // Ajout dans le dictionnaire
                    if (!clientPlusUneCarte.TryGetValue(numeroCompte, out var cartes))
                    {
                        cartes = new List<Apprint>();
                        clientPlusUneCarte[numeroCompte] = cartes;
                    }

                    // ajout d'une nouvelle carte à la liste des cartes existant du client.
                    cartes.Add(apprint);

                    // log progression tous les 10000
                    if (ligne % 10000 == 0)
                    {
                        //_logger.LogInformation("Traitement APPRINT : {ligne} lignes", ligne);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur ligne APPRINT {ligne}", ligne);

                    throw new Exception(
                        $"Erreur lecture fichier APPRINT ligne "
                    );
                }
            }

            return clientPlusUneCarte;
        }

        /// <summary>
        /// Cette fonction parcours le dictionnaire des clients avec une ou plus ou plusieurs carte(s) gratuite(s).
        /// si dans la liste le client à une seule carte, il ne nous interesse pas, on continue la boucle,
        /// on vérifi si le client a résilié son package, si oui calculer le manque à gagner suivant les cas ou,
        /// - la date de fin de souscription est inf a la période de début d'étude (Dfsoupack < Ddeb) :
        ///      - calculer comme si le client n'avait jamais eu de package : MAG = Sum(Min(T, Tci).PUci i allant de 1 à n ou n représente le nombre de carte du client
        /// - la date de fin de souscription est sup à la période de début d'étude (dsouc > Ddeb) :
        ///     - MAG = Sum(Min(T, Tci).PUci - (Max(Tpkg inter Tcj)).PUj i = 1 à n, j = 1 à k, n le nombre de carte, k le nombre de carte qui remplissent TCj avec le Tpck
        /// - si la ddsou inf ddeb et dfsou est sup à ddeb :
        ///     - MAG = Sum(Min(T, Tci).PUci (Max(Tpck inter T inter Tcj)).PUcj
        /// </summary>
        /// <param name="clientPlusUneCarte"></param>
        /// <param name="dateDersouPackEchus"></param>
        /// <param name="packActifs"></param>
        /// <param name="startPeriod"></param>
        /// <param name="endPeriod"></param>
        /// <param name="typeMagResult"></param>
        /// <param name="bkmvtis"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task MapClientPlusUneCarte(
            Dictionary<string, List<Apprint>> clientPlusUneCarte,
            Dictionary<string, DateDsouPackEchuResponse> dateDersouPackEchus,
            Dictionary<string, PackagesActifsResponse> packActifs,
            DateTime startPeriod,
            DateTime endPeriod,
            TypeMag typeMagResult,
            List<Bkmvti> bkmvtis
        )
        {
            // on boucle sur les clients ayant les cartes gratuites
            foreach (var client in clientPlusUneCarte)
            {
                // clé ncpf de la liste des carte du client
                var ncpf = client.Key;
                // liste des cartes du client associé au ncpf
                var cartesClient = client.Value;
                // si le client n'a qu'une seul carte gratuite, ça ne nous interesse pas/plutard.
                if (cartesClient.Count <= 1)
                {
                    continue;
                }

                var mappingCartePackageResult = BuildCartePackageEchuList(
                    cartesClient,
                    dateDersouPackEchus,
                    ncpf
                );

                // rechercher et retourner le code de la carte qui mappe avec le package
                HashSet<string> codeCartes = mappingCartePackageResult
                    .Where(cartePack =>
                        CardConstants.cartePackage.TryGetValue(
                            cartePack.CodeCarte!,
                            out var packages
                        ) && packages.Contains(cartePack.CodePackage!)
                    )
                    .Select(x => x.CodeCarte!)
                    .ToHashSet();

                // recupérer toutes les cartes dont le code de la carte correspond à celui qui mappe avec le package.
                // nous ordonnons par la suite par ordre croissant pour avoir la carte la plus anciènne en tête de liste.
                List<Apprint> maxCartesClientPackResult = cartesClient
                    .Where(carte => codeCartes.Contains(carte.CodeCarte!))
                    .OrderBy(carte => GetDateCreationCarte(carte.DateCreationCarte))
                    .ToList();

                // liste des cartes à faire basculer :
                HashSet<string> cartesABasculer = maxCartesClientPackResult
                    .Skip(1) // skip(1) exclu la prémière (la carte la plus anciènne) qui mappe avec le package.
                    .Select(c => c.NumCarte!)
                    .ToHashSet();

                if (maxCartesClientPackResult.Count() > 1)
                {
                    // _logger.LogInformation(
                    //     "plus d'une carte mappe le package : " + maxCartesClientPackResult
                    // );
                }

                // est ce que le client de ce numéro de compte a résilié son package?
                if (dateDersouPackEchus.TryGetValue(ncpf, out var PackEchu))
                {
                    // si oui
                    if (PackEchu != null)
                    {
                        // Un client ayant résilié sont package ne peut plus figurer dans les packages actifs
                        // si la liste des packages actifs ne contient pas le ncpf, alors :
                        if (!packActifs.TryGetValue(ncpf, out var pack))
                        {
                            //// exple 260210 pour => 10/02/2026
                            //DateTimeOffset? maxDateCreationCarte = GetDateCreationCarte(
                            //    maxCartesClientPack!.DateCreationCarte
                            //);

                            if (PackEchu.Dfsou < startPeriod)
                            {
                                //ClientAvecPlusUneCartePackResilieEtDfsouInfDdeb(cartesClient, ncpf, startPeriod, endPeriod, typeMagResult);
                                await ClientAvecPlusUneCarteSansPackage(
                                    cartesClient,
                                    ncpf,
                                    startPeriod,
                                    endPeriod,
                                    typeMagResult,
                                    bkmvtis,
                                    cartesABasculer
                                );
                            }
                            // ddsou > Ddeb et Dfsou < Ddeb
                            else if (
                                PackEchu.Ddsou > startPeriod
                                && PackEchu.Dfsou > startPeriod
                                && PackEchu.Dfsou < endPeriod
                            )
                            {
                                var periodePack = new Periode(PackEchu.Ddsou, PackEchu.Dfsou);
                                var periodeEtude = new Periode(startPeriod, endPeriod);

                                var carteMaxIntersection = GetCarteMaxIntersection(
                                    maxCartesClientPackResult,
                                    periodePack,
                                    endPeriod
                                );

                                if (carteMaxIntersection != null)
                                {
                                    // récupération de la date de création de la carte qui à le maximum d'intersection
                                    var maxDateIntersectionCarte = GetDateCreationCarte(
                                        carteMaxIntersection.Carte.DateCreationCarte
                                    );

                                    await ClientAvecPlusUneCartePackResilieEtDdsouSupDdeb(
                                        cartesClient,
                                        maxDateIntersectionCarte,
                                        typeMagResult,
                                        ncpf,
                                        startPeriod,
                                        endPeriod,
                                        bkmvtis,
                                        cartesABasculer
                                    );
                                }
                                else
                                {
                                    _logger.LogWarning(
                                        $"Aucune intersection trouvée pour le client {ncpf} avec la période du package"
                                    );
                                }
                            }
                            // ddsou < Ddeb et Dfsou < Ddeb
                            else if (
                                PackEchu.Ddsou < startPeriod
                                && PackEchu.Dfsou > startPeriod
                                && PackEchu.Dfsou < endPeriod
                            )
                            {
                                var periodePack = new Periode(PackEchu.Ddsou, PackEchu.Dfsou);
                                var periodeEtude = new Periode(startPeriod, endPeriod);

                                var carteMaxIntersection = GetCarteMaxIntersection(
                                    maxCartesClientPackResult,
                                    periodePack,
                                    endPeriod,
                                    periodeEtude
                                );

                                if (carteMaxIntersection != null)
                                {
                                    // récupération de la date de création de la carte qui à le maximum d'intersection
                                    var maxDateIntersectionCarte = GetDateCreationCarte(
                                        carteMaxIntersection.Carte.DateCreationCarte
                                    );

                                    await ClientAvecPlusUneCartePackResilieEtDdsouInfDdeb(
                                        cartesClient,
                                        maxDateIntersectionCarte,
                                        typeMagResult,
                                        ncpf,
                                        startPeriod,
                                        endPeriod,
                                        bkmvtis,
                                        cartesABasculer
                                    );
                                }
                                else
                                {
                                    _logger.LogWarning(
                                        $"Aucune intersection trouvée pour le client {ncpf} avec la période du package"
                                    );
                                }
                            }
                        }
                    }
                }
                // si le client n'a pas resilié son package,
                else
                {
                    // constituer le couple (code, package) exple "011 => 300001", "016 => 100003"... entre les packages actifs et le ncpf
                    // afin de vérifier si au moins une carte match avec un package.
                    var cartePackageResult = BuildCartePackageActifList(
                        cartesClient,
                        packActifs,
                        ncpf
                    );

                    bool hasPackage = cartePackageResult.Any();

                    HashSet<string> cartesBasculer = cartesClient
                        .Where(carte =>
                            cartePackageResult.Any(cp => cp.CodeCarte == carte.CodeCarte)
                        )
                        .Select(carte => carte.NumCarte!)
                        .ToHashSet();

                    // le client à t'il un package?
                    if (hasPackage)
                    {
                        // le client à au moins une carte aligner au package?
                        bool hasAtLeastOneCoherenceCartePackage = cartePackageResult.Any(
                            cartePack =>
                                CardConstants.cartePackage.TryGetValue(
                                    cartePack.CodeCarte!,
                                    out var packages
                                ) && packages.Contains(cartePack.CodePackage!)
                        );

                        // le client a t'il au moins une carte aligné au package
                        // => non
                        //  MAG = sum(min(T,Tci)*PUci; i (1 à n) : n => le nombre de carte)
                        if (!hasAtLeastOneCoherenceCartePackage)
                        {
                            // Pour Chaque carte du client recupérer le manque à gagner suivant le min entre la période de traitement
                            // et la période de validité du package

                            await ClientAvecPlusUneCarteSansPackage(
                                cartesClient,
                                ncpf,
                                startPeriod,
                                endPeriod,
                                typeMagResult,
                                bkmvtis,
                                cartesBasculer
                            );
                        }
                        else
                        {
                            var resultat = GetDDSouPackage(packActifs, ncpf);

                            if (resultat == null)
                            {
                                throw new Exception($"Package introuvable pour {ncpf}");
                            }

                            DateTimeOffset ddsou = resultat.Ddsou;

                            // DEBUG
                            Console.WriteLine(ddsou);

                            // si le client à plus d'une carte, au moins une carte correspond au package.
                            // cas 1 : si la période de début est supérieur a la date de début de souscription au package
                            // MAG = sum(min(T,Tci)*PUci - Max(Min(Tpkg, Tcjpkg))*PUcj
                            // cas 2 : si non
                            // MAG = sum(min(T,Tci)*PUci - Max(Min(T, Tcjpkg))*PUcj; (j (1 à k) : k => nombre de carte qui map avec le package, i (1 à n) : n => le nombre de carte)
                            if (startPeriod < ddsou)
                            {
                                string? codeCarte = cartePackageResult
                                    .FirstOrDefault(cartePack =>
                                        CardConstants.cartePackage.TryGetValue(
                                            cartePack.CodeCarte!,
                                            out var packages
                                        ) && packages.Contains(cartePack.CodePackage!)
                                    )
                                    ?.CodeCarte;
                                // recupération du max du min des cartes (c'est lui qu'on devra exclure dans l'application de la formule)

                                var maxCartesClientPack = cartesClient
                                    .Where(c => c.CodeCarte == codeCarte)
                                    .OrderBy(c => c.DateCreationCarte)
                                    .FirstOrDefault();

                                if (maxCartesClientPack == null)
                                {
                                    // Aucun max(min) entre Tpkg et Tcjpkg
                                    await ClientAvecPlusUneCarteAuMoinsPackPeriodePackInfPeriodeEtudeAucunMaxMin(
                                        cartesClient,
                                        ddsou,
                                        ncpf,
                                        typeMagResult,
                                        startPeriod,
                                        endPeriod,
                                        bkmvtis,
                                        cartesABasculer
                                    );
                                }
                                else
                                {
                                    // exple 260210 pour => 10/02/2026
                                    DateTimeOffset? maxDateCreationCarte = GetDateCreationCarte(
                                        maxCartesClientPack!.DateCreationCarte
                                    );

                                    // au moins un Max(Min) entre Tpkg et Tcjpkg
                                    await ClientAvecPlusUneCarteAuMoinsPackPeriodePackInfPeriodeEtude(
                                        cartesClient,
                                        ddsou,
                                        ncpf,
                                        typeMagResult,
                                        startPeriod,
                                        endPeriod,
                                        maxDateCreationCarte,
                                        bkmvtis,
                                        cartesABasculer
                                    );
                                }
                            }
                            else
                            {
                                string? codeCarte = cartePackageResult
                                    .FirstOrDefault(cartePack =>
                                        CardConstants.cartePackage.TryGetValue(
                                            cartePack.CodeCarte!,
                                            out var packages
                                        ) && packages.Contains(cartePack.CodePackage!)
                                    )
                                    ?.CodeCarte;

                                // recupération du max du min des cartes (c'est lui qu'on devra exclure dans l'application de la formule)
                                var maxCartesClientPack = cartesClient
                                    .Where(cl => // cl : carteClient
                                    {
                                        return cl.CodeCarte == codeCarte;
                                    })
                                    .OrderBy(cl => GetDateCreationCarte(cl.DateCreationCarte))
                                    .FirstOrDefault();

                                if (maxCartesClientPack == null)
                                {
                                    // gérer le cas (continuer, logger, ou utiliser autre stratégie)
                                    await ClientAvecPlusUneCarteAuMoinsPackPeriodePackInfPeriodeEtudeAucunMaxMin(
                                        cartesClient,
                                        ddsou,
                                        ncpf,
                                        typeMagResult,
                                        startPeriod,
                                        endPeriod,
                                        bkmvtis,
                                        cartesABasculer
                                    );
                                }
                                else
                                {
                                    var maxDateCreationCarte = GetDateCreationCarte(
                                        maxCartesClientPack.DateCreationCarte
                                    );

                                    await ClientAvecPlusUneCarteAuMoinsPackPeriodePackInfPeriodeEtude(
                                        cartesClient,
                                        ddsou,
                                        ncpf,
                                        typeMagResult,
                                        startPeriod,
                                        endPeriod,
                                        maxDateCreationCarte,
                                        bkmvtis,
                                        cartesABasculer
                                    );
                                }
                            }
                        }
                    }
                    else
                    {
                        await ClientAvecPlusUneCarteSansPackage(
                            cartesClient,
                            ncpf,
                            startPeriod,
                            endPeriod,
                            typeMagResult,
                            bkmvtis,
                            cartesBasculer
                        );
                    }
                }
            }
        }

        public void AjoutListeClientAvecPlusUneCarte(
            Apprint apprint,
            string numeroCompte,
            Dictionary<string, List<Apprint>> clientPlusUneCarte
        )
        {
            clientPlusUneCarte[numeroCompte]
                .Add(
                    new Apprint
                    {
                        CodeCarte = apprint.CodeCarte,
                        NumCarte = apprint.NumCarte,
                        NomPropCarte = apprint.NomPropCarte,
                        LongNum = apprint.LongNum,
                        VhCodeCarte = apprint.VhCodeCarte,
                        QZero = apprint.QZero,
                        DateCreationCarte = apprint.DateCreationCarte,
                        EstActifCodeTarifNumeroCompte = apprint.EstActifCodeTarifNumeroCompte,
                        NomPrenom = apprint.NomPrenom,
                        LastProp = apprint.LastProp,
                        DateValiditeAgenceCodeDeviseNumeroCompte =
                            apprint.DateValiditeAgenceCodeDeviseNumeroCompte,
                    }
                );
        }

        public async Task ClientAvecPlusUneCarteSansPackage(
            List<Apprint> cartesClient,
            string ncpf,
            DateTime startPeriod,
            DateTime endPeriod,
            TypeMag typeMagResult,
            List<Bkmvti> bkmvtis,
            HashSet<string> cartesABasculer
        )
        {
            var compteDebiteRedevance = await _comptesDebiteRedevCarteRepository.GetByNumerosAsync(
                ncpf
            );

            foreach (var carte in cartesClient)
            {
                //var duree = CalculDuree( GetDateCreationCarte(carte.DateCreationCarte),
                //    startPeriod, endPeriod);

                var periodeFacturation = CalculPeriodeFacturation(
                    GetDateCreationCarte(carte.DateCreationCarte),
                    startPeriod,
                    endPeriod
                );

                var lignesCarte = BuildBkmvti(
                    carte,
                    ncpf,
                    periodeFacturation,
                    typeMagResult,
                    startPeriod,
                    compteDebiteRedevance,
                    cartesABasculer
                );

                // IMPORTANT
                bkmvtis.AddRange(lignesCarte);
            }
        }

        public async Task ClientAvecPlusUneCartePackResilieEtDdsouSupDdeb(
            List<Apprint> cartesClient,
            DateTimeOffset maxDateCreationCarte,
            TypeMag typeMagResult,
            string ncpf,
            DateTime startPeriod,
            DateTime endPeriod,
            List<Bkmvti> bkmvtis,
            HashSet<string> cartesABasculer
        )
        {
            var compteDebiteRedevance = await _comptesDebiteRedevCarteRepository.GetByNumerosAsync(
                ncpf
            );

            foreach (var carte in cartesClient)
            {
                // récupération de la date de création de la carte
                var dateCreationCarte = GetDateCreationCarte(carte.DateCreationCarte);

                var periodeFacturation = CalculPeriodeFacturation(
                    GetDateCreationCarte(carte.DateCreationCarte),
                    startPeriod,
                    endPeriod
                );

                // pour chaque date de création de la carte, est elle égale à la date de création de la
                // carte ayant l'intersection maximal?
                if (dateCreationCarte != maxDateCreationCarte)
                {
                    bkmvtis.AddRange(
                        BuildBkmvti(
                            carte,
                            ncpf,
                            periodeFacturation,
                            typeMagResult,
                            startPeriod,
                            compteDebiteRedevance,
                            cartesABasculer
                        )
                    );
                }
            }
        }

        public async Task ClientAvecPlusUneCartePackResilieEtDdsouInfDdeb(
            List<Apprint> cartesClient,
            DateTimeOffset maxDateCreationCarte,
            TypeMag typeMagResult,
            string ncpf,
            DateTime startPeriod,
            DateTime endPeriod,
            List<Bkmvti> bkmvtis,
            HashSet<string> cartesABasculer
        )
        {
            var compteDebiteRedevance = await _comptesDebiteRedevCarteRepository.GetByNumerosAsync(
                ncpf
            );

            foreach (var carte in cartesClient)
            {
                // récupération de la date de création de la carte
                var dateCreationCarte = GetDateCreationCarte(carte.DateCreationCarte);

                var periodeFacturation = CalculPeriodeFacturation(
                    GetDateCreationCarte(carte.DateCreationCarte),
                    startPeriod,
                    endPeriod
                );

                // pour chaque date de création de la carte, est elle égale à la date de création de la
                // carte ayant l'intersection maximal?
                if (dateCreationCarte != maxDateCreationCarte)
                {
                    bkmvtis.AddRange(
                        BuildBkmvti(
                            carte,
                            ncpf,
                            periodeFacturation,
                            typeMagResult,
                            endPeriod,
                            compteDebiteRedevance,
                            cartesABasculer
                        )
                    );
                }
            }
        }

        public async Task ClientAvecPlusUneCarteAuMoinsPackPeriodePackInfPeriodeEtudeAucunMaxMin(
            List<Apprint> cartesClient,
            DateTimeOffset ddsou,
            string ncpf,
            TypeMag typeMagResult,
            DateTime startPeriod,
            DateTime endPeriod,
            List<Bkmvti> bkmvtis,
            HashSet<string> cartesABasculer
        )
        {
            var compteDebiteRedevance = await _comptesDebiteRedevCarteRepository.GetByNumerosAsync(
                ncpf
            );

            foreach (var carte in cartesClient)
            {
                var periodeFacturation = CalculPeriodeFacturation(
                    GetDateCreationCarte(carte.DateCreationCarte),
                    startPeriod,
                    endPeriod
                );

                var dateCreationCarte = GetDateCreationCarte(carte.DateCreationCarte);

                // si la carte ne correspond pas à la carte la plus anciènne apparteneant au package (Tpkg > T)
                bkmvtis.AddRange(
                    BuildBkmvti(
                        carte,
                        ncpf,
                        periodeFacturation,
                        typeMagResult,
                        startPeriod,
                        compteDebiteRedevance,
                        cartesABasculer
                    )
                );
            }
        }

        public async Task ClientAvecPlusUneCarteAuMoinsPackPeriodePackInfPeriodeEtude(
            List<Apprint> cartesClient,
            DateTimeOffset ddsou,
            string ncpf,
            TypeMag typeMagResult,
            DateTime startPeriod,
            DateTime endPeriod,
            DateTimeOffset? maxDateCreationCarte,
            List<Bkmvti> bkmvtis,
            HashSet<string> cartesABasculer
        )
        {
            var compteDebiteRedevance = await _comptesDebiteRedevCarteRepository.GetByNumerosAsync(
                ncpf
            );

            foreach (var carte in cartesClient)
            {
                var periodeFacturation = CalculPeriodeFacturation(
                    GetDateCreationCarte(carte.DateCreationCarte),
                    startPeriod,
                    endPeriod
                );

                var dateCreationCarte = GetDateCreationCarte(carte.DateCreationCarte);

                // si la carte ne correspond pas à la carte la plus anciènne apparteneant au package (Tpkg > T)
                if (dateCreationCarte != maxDateCreationCarte)
                {
                    bkmvtis.AddRange(
                        BuildBkmvti(
                            carte,
                            ncpf,
                            periodeFacturation,
                            typeMagResult,
                            startPeriod,
                            compteDebiteRedevance,
                            cartesABasculer
                        )
                    );
                }
            }
        }

        public string DesignationCartes(string codeTarifComplet)
        {
            return CardConstants.codeTarifNom.TryGetValue(codeTarifComplet, out var designation)
                ? designation
                : "Nom inconnu";
        }

        public string GetNextSequence()
        {
            _sequence++;

            if (_sequence > MAX_SEQUENCE)
                _sequence = 1;

            return _sequence.ToString("D6");
        }

        public long PrixUnitaireCarte(string codeCarte)
        {
            return CardConstants.cartePrix[codeCarte];
        }

        /// <summary>
        ///  constituer le couple (code, package) exple "(011 => 300001)", "(016 => 100003)"... entre les packages actifs et le ncpf
        ///  afin de vérifier si au moins une carte match avec un package.
        /// </summary>
        /// <param name="cartesClient"></param>
        /// <param name="packActifs"></param>
        /// <param name="ncpf"></param>
        /// <returns></returns>
        public List<CartePackageResult> BuildCartePackageActifList(
            List<Apprint> cartesClient,
            Dictionary<string, PackagesActifsResponse> packActifs,
            string ncpf
        )
        {
            var result = new List<CartePackageResult>();
            if (!packActifs.TryGetValue(ncpf, out var pack))
                return result;

            foreach (var carte in cartesClient)
            {
                result.Add(
                    new CartePackageResult
                    {
                        CodeCarte = carte.CodeCarte!,
                        CodePackage = pack.Cpack,
                    }
                );
            }

            return result;
        }

        public List<CartePackageResult> BuildCartePackageEchuList(
            List<Apprint> cartesClient,
            Dictionary<string, DateDsouPackEchuResponse> packEchu,
            string ncpf
        )
        {
            var result = new List<CartePackageResult>();
            if (!packEchu.TryGetValue(ncpf, out var pack))
                return result;

            foreach (var carte in cartesClient)
            {
                result.Add(
                    new CartePackageResult
                    {
                        CodeCarte = carte.CodeCarte!,
                        CodePackage = pack.Cpack,
                    }
                );
            }

            return result;
        }

        public PackagesActifsResponse? GetDDSouPackage(
            Dictionary<string, PackagesActifsResponse> packActifs,
            string ncpf
        )
        {
            if (packActifs.TryGetValue(ncpf, out var pack))
                return pack;

            return null;
        }

        public Periode MinPeriode(Periode p1, Periode p2)
        {
            // p2 est incluse dans p1 => p2 est le min
            if (p2.Debut >= p1.Debut)
                return p2;
            return p1;
        }

        /// <summary>
        /// Calcule la période de facturation effective d’une carte en tenant compte
        /// de la date de début de carte et de la période demandée.
        /// La méthode détermine l’intersection des périodes applicables, puis calcule
        /// le nombre de mois facturables.
        /// </summary>
        /// <param name="dateDebutCarte">Date de début de validité ou de création de la carte.</param>
        /// <param name="startPeriod">Début de la période de calcul demandée.</param>
        /// <param name="endPeriod">Fin de la période de calcul demandée.</param>
        /// <returns>
        /// Un objet <see cref="PeriodeFacturation"/> contenant la période ajustée de facturation
        /// (début, fin) ainsi que le nombre de mois à facturer.
        /// </returns>
        public PeriodeFacturation CalculPeriodeFacturation(
            DateTimeOffset dateDebutCarte,
            DateTime startPeriod,
            DateTime endPeriod
        )
        {
            const int jourPivot = 10;

            var p1 = new Periode(startPeriod, endPeriod);

            var p2 = new Periode(dateDebutCarte.DateTime, endPeriod);

            var min = MinPeriode(p1, p2);

            int nbMois = NombreMoisFacturation(min.Debut, min.Fin, dateDebutCarte);

            DateTime PremDizaine = new DateTime(min.Debut.Year, min.Debut.Month, jourPivot);

            return new PeriodeFacturation
            {
                Debut = PremDizaine,
                Fin = min.Fin,
                NombreMois = Math.Max(0, nbMois),
            };
        }

        public int NombreMois(DateTimeOffset debut, DateTimeOffset fin)
        {
            if (fin < debut)
                return 0;

            int mois = ((fin.Year - debut.Year) * 12) + (fin.Month - debut.Month);

            // Si le dernier mois n'est pas complet
            if (fin.Day < debut.Day && fin.Month == debut.Month)
            {
                mois--;
            }

            return Math.Max(0, mois);
        }

        /// <summary>
        /// Calcule le nombre de mois de facturation entre une période de début et de fin,
        /// en tenant compte de la date de création de la carte et d’un jour pivot (10).
        /// La logique ajuste le début de facturation selon la date de création pour respecter
        /// les règles métier de proratisation des cycles.
        /// </summary>
        /// <param name="debutPeriode">Date de début de la période de facturation.</param>
        /// <param name="finPeriode">Date de fin de la période de facturation.</param>
        /// <param name="dateCreationCarte">Date de création de la carte utilisée pour ajuster le début de facturation.</param>
        /// <returns>
        /// Nombre de mois facturables sur la période. Retourne 0 si la période est invalide
        /// ou si la date de fin est antérieure au début de facturation.
        /// </returns>
        /// </summary>
        public int NombreMoisFacturation(
            DateTimeOffset debutPeriode,
            DateTimeOffset finPeriode,
            DateTimeOffset dateCreationCarte
        )
        {
            const int jourPivot = 10;
            int mois;

            DateTime PremDizaine = new DateTime(debutPeriode.Year, debutPeriode.Month, jourPivot);

            // =========================
            // Détermine le premier cycle
            // de facturation réel
            // =========================

            DateTime debutFacturation;

            if (dateCreationCarte.DateTime <= PremDizaine)
            {
                // Facturation le 1 du même mois
                debutFacturation = PremDizaine;
            }
            else if (
                dateCreationCarte.Day >= jourPivot
                && dateCreationCarte.DateTime >= PremDizaine
            )
            {
                // Facturation le 12 du mois suivant
                var moisSuivant = debutPeriode.AddMonths(1);

                debutFacturation = new DateTime(moisSuivant.Year, moisSuivant.Month, jourPivot);
            }
            else
            {
                debutFacturation = debutPeriode.DateTime;
            }

            // Fin réelle de calcul

            var finFacturation = new DateTime(finPeriode.Year, finPeriode.Month, finPeriode.Day);

            // Sécurité la date de fin ne saurait être antérieur à la date de début.

            if (finFacturation <= debutFacturation)
                return 0;

            // Calcul exact des cycles

            if (finFacturation.Day < jourPivot)
            {
                mois =
                    ((finFacturation.Year - debutFacturation.Year) * 12)
                    + (finFacturation.Month - debutFacturation.Month);
            }
            else
            {
                mois =
                    ((finFacturation.Year - debutFacturation.Year) * 12)
                    + (finFacturation.Month - debutFacturation.Month)
                    + 1;
            }
            _logger.LogInformation(
                $"nombre de mois de facturation : {mois}, début : {debutFacturation}, fin : {finFacturation}"
            );
            return mois;
        }

        /// <summary>
        /// Calcule l’intersection commune entre plusieurs périodes temporelles.
        ///
        /// La méthode détermine :
        /// - la date de début maximale parmi les périodes,
        /// - la date de fin minimale parmi les périodes,
        /// afin d’obtenir la zone de recouvrement effective.
        ///
        /// Si aucune intersection n’existe (périodes disjointes),
        /// la méthode retourne <c>null</c>.
        ///
        /// Cette méthode est utilisée pour les calculs de couverture
        /// temporelle (ex : périodes de carte, package et étude).
        ///
        /// </summary>
        /// <param name="periodes">
        /// Ensemble de périodes à intersecter.
        /// </param>
        /// <returns>
        /// Une <see cref="Periode"/> représentant l’intersection commune,
        /// ou <c>null</c> si aucune intersection n’existe.
        /// </returns>
        public Periode? GetIntersectionPeriode(params Periode[] periodes)
        {
            if (periodes == null || periodes.Length == 0)
                return null;

            var debutMax = periodes.Max(p => p.Debut);
            var finMin = periodes.Min(p => p.Fin);

            if (debutMax > finMin)
                return null; // pas d'intersection

            return new Periode(debutMax, finMin);
        }

        public int GetDureeMois(Periode p)
        {
            return (p.Fin.Year - p.Debut.Year) * 12 + (p.Fin.Month - p.Debut.Month);
        }

        /// <summary>
        /// Détermine, parmi les cartes d’un client, celle dont la période
        /// d’intersection avec la période du package (et éventuellement la
        /// période d’étude) est la plus longue.
        ///
        /// Le calcul consiste à :
        /// 1. Construire la période de validité théorique de chaque carte
        ///    à partir de sa date de création jusqu’à la fin de période.
        /// 2. Calculer l’intersection entre :
        ///    - la période de la carte,
        ///    - la période du package,
        ///    - et éventuellement la période d’étude.
        /// 3. Conserver la carte ayant la durée d’intersection maximale.
        /// 4. En cas d’égalité, retourner la carte la plus ancienne.
        ///
        /// Cette méthode est utilisée pour identifier la carte à exclure
        /// du calcul du manque à gagner lorsqu’un package couvre déjà
        /// une des cartes du client.
        /// </summary>
        /// <param name="cartesClient">
        /// Liste des cartes associées au client.
        /// </param>
        /// <param name="periodePackage">
        /// Période de validité ou de souscription du package.
        /// </param>
        /// <param name="endPeriode">
        /// Date de fin  de la période d'étude.
        /// </param>
        /// <param name="periodeEtude">
        /// Période de début d'étude.
        /// </param>
        /// <returns>
        /// Retourne la carte ayant la plus grande durée d’intersection,
        /// ainsi que la période d’intersection correspondante.
        /// Retourne <c>null</c> si aucune intersection n’existe.
        /// </returns>
        public CarteIntersectionResult? GetCarteMaxIntersection(
            List<Apprint> cartesClient,
            Periode periodePackage,
            DateTimeOffset endPeriode,
            Periode? periodeEtude = null
        )
        {
            var listeIntersectionCarte = cartesClient
                .Select(carte =>
                {
                    var dateCreation = GetDateCreationCarte(carte.DateCreationCarte);

                    var periodeCarte = new Periode(dateCreation, endPeriode);

                    Periode? intersection;

                    if (periodeEtude != null)
                    {
                        intersection = GetIntersectionPeriode(
                            periodeCarte,
                            periodePackage,
                            periodeEtude
                        );
                    }
                    else
                    {
                        intersection = GetIntersectionPeriode(periodeCarte, periodePackage);
                    }

                    if (intersection == null)
                        return null;

                    return new
                    {
                        Carte = carte,
                        Intersection = intersection,
                        Duree = GetDureeMois(intersection),
                        DateCreation = dateCreation,
                    };
                })
                .Where(x => x != null)
                .ToList();

            if (!listeIntersectionCarte.Any())
                return null;

            var maxDuree = listeIntersectionCarte.Max(x => x!.Duree);

            var MaxIntersection = listeIntersectionCarte
                .Where(x => x!.Duree == maxDuree)
                .OrderBy(x => x!.DateCreation)
                .First();

            return new CarteIntersectionResult(
                MaxIntersection!.Carte,
                MaxIntersection.Intersection
            );
        }

        public bool CarteDejaPrelevee(
            string numeroCompte,
            string mois,
            decimal montantCarte,
            Dictionary<string, List<CompteDebiteRedevCarte>> redevances
        )
        {
            if (!redevances.ContainsKey(numeroCompte))
                return false;

            return redevances[numeroCompte].Any(x => x.Dco == mois && x.Mon == montantCarte);
        }

        /// <summary>
        /// Génère la liste des écritures comptables BKMVTI pour une carte donnée
        /// sur une période de facturation.
        ///
        /// La méthode produit, mois par mois, les écritures de redevance carte
        /// en tenant compte :
        /// - de la période de facturation calculée,
        /// - des prélèvements déjà effectués,
        /// - du prix unitaire de la carte,
        /// - et des règles métier associées au type de carte.
        ///
        /// Chaque itération du cycle mensuel génère une écriture comptable
        /// de type débit si la redevance n’a pas déjà été prélevée.
        ///
        /// Les informations générées incluent notamment :
        /// - les références de compte,
        /// - les codes tarifaires et produits,
        /// - les dates de début et fin de période,
        /// - le jour du prélèvement
        /// - le montant unitaire,
        /// - ...
        ///
        /// Les mois déjà prélevés sont ignorés afin d’éviter
        /// toute double facturation.
        ///
        /// </summary>
        /// <param name="carte">
        /// Informations de la carte APPRINT traitée.
        /// </param>
        /// <param name="ncpf">
        /// Numéro de compte client.
        /// </param>
        /// <param name="periodeFacturation">
        /// Période calculée de facturation incluant le nombre de mois
        /// et la date de début effective.
        /// </param>
        /// <param name="typeMagResult">
        /// Type de traitement MAG utilisé pour associer les écritures.
        /// </param>
        /// <param name="startPeriod">
        /// Date de début de la période d’étude.
        /// </param>
        /// <param name="redevancesPrelevees">
        /// Historique des redevances déjà prélevées par compte et par mois,
        /// utilisé pour éviter les doublons.
        /// </param>
        /// <returns>
        /// Une liste d’écritures comptables BKMVTI correspondant
        /// aux redevances carte à générer.
        /// </returns>
        public List<Bkmvti> BuildBkmvti(
            Apprint carte,
            string ncpf,
            PeriodeFacturation periodeFacturation,
            TypeMag typeMagResult,
            DateTime startPeriod,
            Dictionary<string, List<CompteDebiteRedevCarte>> redevancesPrelevees,
            HashSet<string> cartesABasculer
        )
        {
            var result = new List<Bkmvti>();

            bool basculer = cartesABasculer.Contains(carte.NumCarte!);

            string codeTarif = carte.EstActifCodeTarifNumeroCompte!.Substring(1, 2);

            string codeTarifComplet = BuildCodeTarifComplet(
                carte.EstActifCodeTarifNumeroCompte,
                carte.CodeCarte
            );

            string designationCarte = DesignationCartes(codeTarifComplet);

            long prixMensuelCarte = PrixUnitaireCarte(carte.CodeCarte!);

            // GENERATION MOIS PAR MOIS

            for (int i = 0; i < periodeFacturation.NombreMois; i++)
            {
                var moisFacture = periodeFacturation.Debut.AddMonths(i);

                if (moisFacture > periodeFacturation.Debut.AddMonths(i))
                {
                    break;
                }

                string moisKey = moisFacture.ToString("yyyy-MM");

                // Vérifie si déjà débité

                bool dejaPreleve = CarteDejaPrelevee(
                    ncpf,
                    moisKey,
                    prixMensuelCarte,
                    redevancesPrelevees
                );

                if (dejaPreleve)
                {
                    // _logger.LogInformation(
                    //     $"SKIP => {ncpf} | {designationCarte} | {moisKey} déjà prélevé"
                    // );
                    continue;
                }

                // Génération comptable

                result.Add(
                    new Bkmvti
                    {
                        NumeroCompte = ncpf,

                        NomClient = carte.NomPrenom,

                        DateCreationCarte = DateTime.SpecifyKind(
                            GetDateCreationCarte(carte.DateCreationCarte).DateTime,
                            DateTimeKind.Utc
                        ),

                        DateValiditeCarte = DateTime.SpecifyKind(
                            GetDateValiditeCarte(carte).DateTime,
                            DateTimeKind.Utc
                        ),

                        CodeTarif = codeTarif,

                        CodeCarte = carte.CodeCarte!,

                        Basculer = basculer,

                        DesignationCarte = designationCarte,

                        StartPeriod = DateTime.SpecifyKind(moisFacture.Date, DateTimeKind.Utc),

                        EndPeriod = DateTime.SpecifyKind(moisFacture.Date, DateTimeKind.Utc),

                        TypeMag = typeMagResult.Id,

                        CodeIN = "IN3",

                        CodeDevise = carte.DateValiditeAgenceCodeDeviseNumeroCompte!.Substring(
                            9,
                            3
                        ),

                        EstActif = carte.EstActifCodeTarifNumeroCompte!.Substring(0, 1),

                        CodeAgence = carte.DateValiditeAgenceCodeDeviseNumeroCompte!.Substring(
                            4,
                            5
                        ),

                        TypeBeneficiaire = " AUTO",

                        ReferenceBeneficiaire = 691228,

                        CleBeneficiaire = 46,

                        DatePrelevement = DateTime.SpecifyKind(moisFacture.Date, DateTimeKind.Utc),

                        // IMPORTANT :
                        // un seul mois
                        PrixUnitCarte = prixMensuelCarte,

                        ReferenceOperation = $"RVSA{moisFacture:yyMMdd}",

                        CodeOperation = "D",

                        CodeEmetteur = "FACSER",

                        IndicateurDomiciliation = "N",

                        LibelleCarte = BuildLibelleCarte(
                            carte.EstActifCodeTarifNumeroCompte,
                            carte.CodeCarte,
                            moisFacture.Date
                        ),

                        Carte = carte.NumCarte!.Substring(8),

                        Sequence = "001",
                    }
                );
            }

            return result;
        }

        /// <summary>
        /// Construit le code tarifaire complet d’une carte en concaténant :
        /// - le code de tarification extrait du champ
        ///   <c>EstActifCodeTarifNumeroCompte</c>,
        /// - et le code produit de la carte.
        ///
        /// Ce code permet d’identifier précisément le type de carte
        /// afin de retrouver son libellé métier
        /// (exemple : Visa Premier, Visa Platinum, etc.).
        ///
        /// Exemple :
        /// <code>
        /// Code tarif : "CL"
        /// Code carte : "011"
        /// Résultat   : "CL011"
        /// </code>
        /// </summary>
        /// <param name="estActifCodeTarifNumeroCompte">
        /// Chaîne contenant les informations d’état de la carte,
        /// le code tarifaire et le numéro de compte.
        /// Le code tarifaire est extrait à partir des positions 1 à 2.
        /// </param>
        /// <param name="codeCarte">
        /// Code produit de la carte.
        /// Exemples : 011, 016, 006, 007.
        /// </param>
        /// <returns>
        /// Le code tarifaire complet permettant d’identifier
        /// le type exact de carte.
        /// Retourne une chaîne vide si un des paramètres est nul ou vide.
        /// </returns>
        public string BuildCodeTarifComplet(
            string? estActifCodeTarifNumeroCompte,
            string? codeCarte
        )
        {
            if (
                string.IsNullOrEmpty(estActifCodeTarifNumeroCompte)
                || string.IsNullOrEmpty(codeCarte)
            )
                return string.Empty;

            string codeTarif = estActifCodeTarifNumeroCompte.Substring(1, 2);
            return codeTarif + codeCarte;
        }

        /// <summary>
        /// Construit le libellé métier d’une redevance carte
        /// à partir du code tarifaire complet et de la période
        /// de prélèvement.
        ///
        /// Le libellé généré est utilisé dans les écritures comptables
        /// et les fichiers BKMVTI.
        ///
        /// Exemple de résultat :
        /// <code>
        /// Regul. redev. Visa Premier févr. 2026
        /// </code>
        ///
        /// La méthode :
        /// - construit d’abord la clé tarifaire complète,
        /// - recherche ensuite le libellé correspondant
        ///   dans le dictionnaire des correspondances métiers,
        /// - puis ajoute le mois et l’année de facturation.
        ///
        /// Si aucun libellé n’est trouvé pour la clé,
        /// un libellé générique est retourné et un avertissement
        /// est journalisé.
        ///
        /// </summary>
        /// <param name="estActifCodeTarifNumeroCompte">
        /// Chaîne contenant notamment le code tarifaire de la carte.
        /// </param>
        /// <param name="codeCarte">
        /// Code produit de la carte.
        /// </param>
        /// <param name="startPeriod">
        /// Date représentant la période de facturation utilisée
        /// pour construire le mois et l’année du libellé.
        /// </param>
        /// <returns>
        /// Le libellé formaté de la redevance carte.
        /// </returns>
        public string BuildLibelleCarte(
            string? estActifCodeTarifNumeroCompte,
            string? codeCarte,
            DateTime startPeriod
        )
        {
            string key = BuildCodeTarifComplet(estActifCodeTarifNumeroCompte, codeCarte);

            if (string.IsNullOrEmpty(key))
                return "Nom inconnu";

            string mois = RemoveAccents(startPeriod.ToString("MMM", new CultureInfo("fr-FR")));

            if (CardConstants.codeTarifNom.TryGetValue(key, out var libelle))
            {
                return $"Regul redev. {libelle} {mois} {startPeriod.Year}";
            }
            else
            {
                _logger.LogWarning("Libellé tarif introuvable pour la clé: {Key}", key);

                return $"Regul redev. {mois} {startPeriod.Year}";
            }
        }

        public static string RemoveAccents(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var normalized = text.Normalize(NormalizationForm.FormD);

            var builder = new StringBuilder();

            foreach (char c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);

                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(c);
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Convertit la date de création d’une carte provenant du fichier
        /// APPRINT au format <c>yyMMdd</c> en <see cref="DateTimeOffset"/>.
        ///
        /// Exemple :
        /// <code>
        /// 260210 => 10/02/2026
        /// </code>
        ///
        /// Cette date est utilisée dans les calculs de :
        /// - période de facturation,
        /// - intersection de périodes,
        /// - détermination du manque à gagner.
        ///
        /// Une exception est levée si la date ne respecte pas
        /// le format attendu.
        /// </summary>
        /// <param name="dNaissanceCarte">
        /// Date de création de la carte au format texte <c>yyMMdd</c>.
        /// </param>
        /// <returns>
        /// La date de création convertie en <see cref="DateTimeOffset"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Levée lorsque la date est invalide ou ne respecte pas
        /// le format <c>yyMMdd</c>.
        /// </exception>
        public DateTimeOffset GetDateCreationCarte(string dNaissanceCarte)
        {
            if (
                DateTime.TryParseExact(
                    dNaissanceCarte,
                    "yyMMdd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime parsedDate
                )
            )
            {
                return parsedDate;
            }

            _logger.LogInformation("Date invalide : " + dNaissanceCarte);
            throw new Exception("");
        }

        //public DateTimeOffset GetDateValiditeCarte(Apprints apprint)
        //{
        //    // EXTRACTION DE LA DATE DE VALIDITE DE LA CARTE
        //    var dateValiditeAgenceCodeDeviseNumeroCompte =
        //        apprint.DateValiditeAgenceCodeDeviseNumeroCompte;
        //    var annee = int.Parse(dateValiditeAgenceCodeDeviseNumeroCompte!.Substring(0, 2));
        //    var mois = int.Parse(dateValiditeAgenceCodeDeviseNumeroCompte.Substring(2, 2));
        //    annee += 2000;
        //    _logger.LogInformation("date de validite carte : " + mois + "/" + annee);
        //    // Création de la date avec le 1er jour du mois
        //    DateTime dateValiditeCarte = new DateTime(annee, mois, 1);
        //    // Conversion en DateTimeOffset avec le décalage local
        //    DateTimeOffset dtoLocal = new DateTimeOffset(dateValiditeCarte);
        //    _logger.LogInformation("date de validite carte : " + dtoLocal);
        //    return dtoLocal;
        //}

        public DateTimeOffset GetDateValiditeCarte(Apprint apprint)
        {
            var valeur = apprint.DateValiditeAgenceCodeDeviseNumeroCompte;

            if (string.IsNullOrWhiteSpace(valeur) || valeur.Length < 4)
                throw new Exception("Date validité carte invalide");

            var annee = 2000 + int.Parse(valeur.Substring(0, 2));
            var mois = int.Parse(valeur.Substring(2, 2));

            var dernierJour = DateTime.DaysInMonth(annee, mois);

            var dateValiditeCarte = new DateTimeOffset(
                annee,
                mois,
                dernierJour,
                23,
                59,
                59,
                TimeSpan.Zero
            );

            //_logger.LogInformation("Date validité carte calculée : {Date}", dateValiditeCarte);

            return dateValiditeCarte;
        }

        public Apprint ConvertTxtToApprint(string ligneApprint, long numLigne)
        {
            if (string.IsNullOrWhiteSpace(ligneApprint))
                return null!;

            try
            {
                // ligne représente une ligne de l'apprint (de la base des cartes) qui doit être parser
                ligneApprint = ligneApprint.PadRight(260);

                // fonction qui retourne le parsing du fichier de l'apprint.
                string Get(int start, int length)
                {
                    if (ligneApprint.Length < start + length)
                        return string.Empty;

                    return ligneApprint.Substring(start, length).Trim();
                }

                var apprint = new Apprint
                {
                    NumCarte = Get(0, 24),
                    NomPropCarte = Get(30, 39),
                    LongNum = Get(30, 39),
                    VhCodeCarte = Get(70, 5),
                    QZero = Get(77, 4),

                    DateValiditeAgenceCodeDeviseNumeroCompte = Get(93, 24),

                    DateCreationCarte = Get(154, 6),

                    EstActifCodeTarifNumeroCompte = Get(161, 23),

                    CodeCarte = Get(193, 3),

                    NomPrenom = Get(197, 26),

                    LastProp = Get(229, 29),
                };

                bool ligneInvalide =
                    string.IsNullOrWhiteSpace(apprint.DateValiditeAgenceCodeDeviseNumeroCompte)
                    || apprint.DateValiditeAgenceCodeDeviseNumeroCompte.Length < 20;

                if (ligneInvalide)
                {
                    _logger.LogWarning(
                        $@"
                    ================ LIGNE APPRINT INVALIDE ================
                    Numéro ligne : {numLigne}

                    Ligne brute :
                    {ligneApprint}

                    Valeur extraite :
                    [{apprint.DateValiditeAgenceCodeDeviseNumeroCompte}]

                    Longueur :
                    {apprint.DateValiditeAgenceCodeDeviseNumeroCompte?.Length}
                    ========================================================
                    "
                    );

                    // Recherche MMYY + espaces + compte 19 chiffres
                    var match = Regex.Match(ligneApprint, @"(?<date>\d{4})\s+(?<compte>\d{19})");

                    if (match.Success)
                    {
                        string dateValidite = match.Groups["date"].Value;
                        string compte = match.Groups["compte"].Value;

                        apprint.DateValiditeAgenceCodeDeviseNumeroCompte =
                            $"{dateValidite}{compte}";

                        // _logger.LogInformation(
                        //     $"Correction automatique appliquée ligne {numLigne} : {apprint.DateValiditeAgenceCodeDeviseNumeroCompte}"
                        // );
                    }
                    else
                    {
                        _logger.LogError(
                            $"Impossible de corriger automatiquement la ligne {numLigne}"
                        );
                    }
                }

                return apprint;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $@"
                    Erreur parsing ligne APPRINT

                    Ligne numéro : {numLigne}

                    Contenu :
                    {ligneApprint}

                    Erreur :
                    {ex.Message}
                    "
                );

                throw;
            }
        }

        /// <summary>
        /// Génère le fichier comptable final au format texte à partir
        /// des lignes de manque à gagner calculées.
        ///
        /// Le fichier contient :
        ///
        /// 1. Les lignes de débit client
        ///    correspondant aux redevances cartes à prélever.
        ///
        /// 2. Les lignes de crédit par agence :
        ///    - une ligne de crédit TVA,
        ///    - une ligne de crédit Commission.
        ///
        /// Le traitement est regroupé par agence afin de :
        /// - calculer le montant total des prélèvements,
        /// - répartir automatiquement la TVA et la commission,
        /// - générer les écritures comptables associées.
        ///
        /// Les comptes TVA et Commission sont récupérés à partir
        /// des comptes ouverts de chaque agence. (par leur clé)
        ///
        /// Le résultat final est converti en tableau d’octets UTF-8
        /// afin de permettre :
        /// - le téléchargement du fichier,
        /// - l’écriture sur disque,
        /// - ou l’envoi vers un système comptable.
        ///
        /// </summary>
        /// <param name="bkmvtis">
        /// Liste des écritures de manque à gagner à transformer
        /// en lignes comptables.
        /// </param>
        /// <returns>
        /// Un tableau d’octets représentant le contenu complet
        /// du fichier comptable généré.
        /// </returns>
        public async Task<byte[]> GenerateFile(List<BkmvtiResult> bkmvtis)
        {
            var lignes = new List<string>();

            var compteOuvert = await _comptesOuvertService.GetAllComptesOuvertsAsync();

            var comptesDict = compteOuvert.ToDictionary(c => (c.Ncp, c.Age), c => c);

            // LIGNES DEBIT CLIENTS

            foreach (var item in bkmvtis)
            {
                var line = BuildDebitLine(item, comptesDict);
                if (!string.IsNullOrEmpty(line))
                    //_logger.LogInformation(line);

                    lignes.Add(line);
            }

            // GROUP BY UNIQUEMENT PAR AGENCE

            var groupesAgence = bkmvtis.GroupBy(x => x.CodeAgence);

            foreach (var agenceGroup in groupesAgence)
            {
                var tvaAndCommission = await _comptesOuvertService.GetByAgeAsync(agenceGroup.Key!);

                var codeAgence = agenceGroup.Key!;

                var totalAgence = agenceGroup.Sum(x => x.Montant ?? 0);

                var montantTva = Math.Round(totalAgence * AccountingConstants.TvaRate);

                var montantCommission = totalAgence - montantTva;

                var datePrelevement = DateTime.Now;

                // TVA
                lignes.Add(
                    BuildCreditLine(
                        codeAgence,
                        tvaAndCommission[1].Ncp,
                        montantTva,
                        "Regul Redev " + tvaAndCommission[1].Inti.Substring(0, 13),
                        tvaAndCommission[1].Cha,
                        tvaAndCommission[1].Clc,
                        datePrelevement
                    )
                );

                // COMMISSION
                lignes.Add(
                    BuildCreditLine(
                        codeAgence,
                        tvaAndCommission[0].Ncp,
                        montantCommission,
                        "Regul Redev " + tvaAndCommission[0].Inti.Substring(0, 17),
                        tvaAndCommission[0].Cha,
                        tvaAndCommission[0].Clc,
                        datePrelevement
                    )
                );
            }

            var contenu = string.Join(Environment.NewLine, lignes);

            return Encoding.UTF8.GetBytes(contenu);
        }

        public async Task<bool> IsDownloadAsync(Guid typeMagId)
        {
            return await _typeMagRepository.IsDownload(typeMagId);
        }

        /// <summary>
        /// Construit une ligne comptable de débit au format BKMVTI
        /// correspondant à une redevance carte à prélever sur le compte client.
        ///
        /// La méthode :
        /// - récupère les informations du compte ouvert associé au client,
        /// - construit les différents champs comptables attendus,
        /// - formate la ligne selon la structure BKMVTI séparée par le caractère '|'.
        ///
        /// La ligne générée représente une écriture de débit client
        /// utilisée dans le fichier comptable final.
        ///
        /// Les informations construites incluent notamment :
        /// - le compte client,
        /// - le chapitre comptable (CHA),
        /// - la clé comptable (CLC),
        /// - le montant du prélèvement,
        /// - le libellé de l’opération,
        /// - le numéro d'evenement,
        /// - ...
        ///
        ///
        /// </summary>
        /// <param name="bkmvtis">
        /// Objet contenant les informations métier nécessaires
        /// à la génération de la ligne comptable de débit.
        /// </param>
        /// <returns>
        /// Une chaîne formatée représentant une ligne BKMVTI de débit.
        ///
        /// Retourne <c>null</c> si le compte client est introuvable.
        /// </returns>
        public string? BuildDebitLine(
            BkmvtiResult bkmvtis,
            Dictionary<(string Ncp, string Age), ComptesOuvert> comptesDict
        )
        {
            // Recherche rapide dans le dictionnaire
            if (!comptesDict.TryGetValue((bkmvtis.NumeroCompte, bkmvtis.CodeAgence), out var cpt))
            {
                return null;
            }

            var bkmvti = new List<string>
            {
                bkmvtis.CodeAgence,
                AccountingConstants.CurrencyCode,
                cpt!.Cha,
                bkmvtis.NumeroCompte,
                " ",
                AccountingConstants.CodeIn,
                " ",
                " ",
                AccountingConstants.BeneficiaryType,
                GetNextSequence(),
                cpt.Clc,
                "04/10/2025",
                //DateTime.Now.ToString("dd/MM/yyyy"),

                " ",
                "04/10/2025",
                //DateTime.Now.ToString("dd/MM/yyyy"),

                bkmvtis.Montant.ToString()!,
                AccountingConstants.DebitOperation,
                bkmvtis.LibelleCarte!,
                "N",
                $"RVSA20251004",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                bkmvtis.CodeAgence,
                " ",
                " ",
                AccountingConstants.CurrencyCode,
                " ",
                " ",
                bkmvtis.CodeAgence + AccountingConstants.DebitOperation,
                " ",
                AccountingConstants.CurrencyCode,
                " ",
                " ",
                " ",
                " ",
                " ",
                AccountingConstants.EmitterCode,
                " ",
                " ",
                " ",
                " ",
                " ",
            };

            return string.Join("|", bkmvti);
        }

        public string BuildCreditLine(
            string codeAgence,
            string numeroCompte,
            decimal montant,
            string libelle,
            string cha,
            string clc,
            DateTime datePrelevement
        )
        {
            var bkmvti = new List<string>
            {
                codeAgence,
                AccountingConstants.CurrencyCode,
                cha,
                numeroCompte,
                " ",
                AccountingConstants.CodeIn,
                " ",
                " ",
                AccountingConstants.BeneficiaryType,
                GetNextSequence(),
                clc,
                "04/10/2025",
                //DateTime.Now.ToString("dd/MM/yyyy"),

                " ",
                "04/10/2025",
                //DateTime.Now.ToString("dd/MM/yyyy"),

                montant.ToString(),
                AccountingConstants.CreditOperation,
                libelle,
                "N",
                $"RVSA20251004",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                " ",
                codeAgence,
                " ",
                " ",
                AccountingConstants.CurrencyCode,
                " ",
                " ",
                codeAgence + AccountingConstants.CreditOperation,
                " ",
                AccountingConstants.CurrencyCode,
                " ",
                " ",
                " ",
                " ",
                " ",
                AccountingConstants.EmitterCode,
                " ",
                " ",
                " ",
                " ",
                " ",
            };

            return string.Join("|", bkmvti);
        }

        /// <summary>
        /// Lit la feuille Excel des comptes actifs et construit
        /// un dictionnaire indexé par numéro de compte.
        ///
        /// Chaque ligne de la feuille représente un compte actif.
        /// La clé du dictionnaire correspond au numéro de compte (NCP),
        /// ce qui permet des recherches rapides lors des traitements métier.
        ///
        /// Le parcours commence à la ligne 2 afin d’ignorer
        /// la ligne d’en-tête du fichier Excel.
        ///
        /// </summary>
        /// <param name="worksheetCompteActif">
        /// Feuille Excel contenant la liste des comptes actifs.
        /// </param>
        /// <returns>
        /// Un dictionnaire dont :
        /// - la clé est le numéro de compte (NCP),
        /// - la valeur est un objet <see cref="ComptesActifsResponse"/>.
        ///
        /// Les comptes vides ou invalides sont ignorés.
        /// </returns>
        public Dictionary<string, ComptesActifsResponse> GetComptesActifs(
            ExcelWorksheet worksheetCompteActif
        )
        {
            var comptesActif = new Dictionary<string, ComptesActifsResponse>();
            // commencer par la ligne 2 si la ligne 1 est l'en-tête
            try
            {
                // VALIDATION HEADER
                string headerNcp = worksheetCompteActif.Cells[1, 1].Text.Trim();

                if (headerNcp != "NCP")
                {
                    throw new Exception(
                        "Le fichier chargé n'est pas un fichier Compte Actif valide."
                    );
                }

                for (int row = 2; row <= worksheetCompteActif.Dimension.End.Row; row++)
                {
                    string ncp = worksheetCompteActif.Cells[row, 1].Text.Trim();
                    if (!string.IsNullOrEmpty(ncp))
                    {
                        comptesActif[ncp] = new ComptesActifsResponse { Ncp = ncp };
                    }
                }
                return comptesActif;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement du fichier Compte Actif");

                throw new Exception("Erreur lors du chargement du fichier Compte Actif");
            }
        }

        public Dictionary<string, ComptesCtxResponse> GetComptesCtx(
            ExcelWorksheet worksheetCompteCtx
        )
        {
            var comptesCtx = new Dictionary<string, ComptesCtxResponse>();
            try
            {
                // VALIDATION HEADER
                string headerNcp = worksheetCompteCtx.Cells[1, 1].Text.Trim();

                if (headerNcp != "NCP")
                {
                    throw new Exception(
                        "Le fichier chargé n'est pas un fichier Compte contentieux valide."
                    );
                }
                // commencer par la ligne 2 si la ligne 1 est l'en-tête
                for (int row = 2; row <= worksheetCompteCtx.Dimension.End.Row; row++)
                {
                    string ncp = worksheetCompteCtx.Cells[row, 1].Text.Trim();
                    if (!string.IsNullOrEmpty(ncp))
                    {
                        comptesCtx[ncp] = new ComptesCtxResponse { Ncp = ncp };
                    }
                }
                return comptesCtx;
            }
            catch (Exception)
            {
                _logger.LogError("Fichier de Comptes en comptentieux invalide");
                throw new Exception("Fichier de Comptes en comptentieux invalide" );
            }
        }

        public List<ComptesOuvert> GetComptesOuvert(ExcelWorksheet worksheetCompteOuvert)
        {
            var comptesOuvert = new List<ComptesOuvert>();
            try
            {
                // VALIDATION HEADER
                string headerNcp = worksheetCompteOuvert.Cells[1, 1].Text.Trim();
                string headerCFE = worksheetCompteOuvert.Cells[1, 4].Text.Trim();
                string headerCLC = worksheetCompteOuvert.Cells[1, 2].Text.Trim();
                string headerCHA = worksheetCompteOuvert.Cells[1, 3].Text.Trim();
                string headerAGE = worksheetCompteOuvert.Cells[1, 5].Text.Trim();
                string headerINTI = worksheetCompteOuvert.Cells[1, 6].Text.Trim();

                if (
                    headerNcp != "NCP"
                    || headerAGE != "AGE"
                    || headerCFE != "CFE"
                    || headerCHA != "CHA"
                    || headerCLC != "CLC"
                    || headerINTI != "INTI"
                )
                {
                    throw new Exception(
                        "Le fichier chargé n'est pas un fichier Compte Ouvert valide."
                    );
                }
                // commencer par la ligne 2 si la ligne 1 est l'en-tête
                for (int row = 2; row <= worksheetCompteOuvert.Dimension.End.Row; row++)
                {
                    string ncp = worksheetCompteOuvert.Cells[row, 1].Text.Trim();
                    if (!string.IsNullOrEmpty(ncp))
                    {
                        comptesOuvert.Add(
                            new ComptesOuvert
                            {
                                Ncp = ncp,
                                Cfe = worksheetCompteOuvert.Cells[row, 4].Text.Trim(),
                                Clc = worksheetCompteOuvert.Cells[row, 2].Text.Trim(),
                                Cha = worksheetCompteOuvert.Cells[row, 3].Text.Trim(),
                                Age = worksheetCompteOuvert.Cells[row, 5].Text.Trim(),
                                Inti = worksheetCompteOuvert.Cells[row, 6].Text.Trim(),
                            }
                        );
                    }
                }
                return comptesOuvert;
            }
            catch (Exception)
            {
                _logger.LogError("Fichier de Comptes Ouvert invalide");
                throw new Exception("Fichier de Comptes Ouvert invalide");
            }
        }

        public Dictionary<string, DateDsouPackEchuResponse> GetDsouPackEchu(
            ExcelWorksheet worksheetDsouPackEchu
        )
        {
            var dateDsouPackEchu = new Dictionary<string, DateDsouPackEchuResponse>();

            try
            {
                // VALIDATION HEADER
                string headerNcpf = worksheetDsouPackEchu.Cells[1, 1].Text.Trim();
                string headerCpack = worksheetDsouPackEchu.Cells[1, 2].Text.Trim();
                string headerDdsou = worksheetDsouPackEchu.Cells[1, 3].Text.Trim();
                string headerDfsou = worksheetDsouPackEchu.Cells[1, 4].Text.Trim();

                if (
                    headerNcpf != "NCPF"
                    || headerCpack != "CPACK"
                    || headerDdsou != "DDSOU"
                    || headerDfsou != "DFSOU"
                )
                {
                    throw new Exception(
                        "Le fichier chargé n'est pas un fichier Package Echu valide."
                    );
                }
                // commencer par la ligne 2 si la ligne 1 est l'en-tête
                for (int row = 2; row <= worksheetDsouPackEchu.Dimension.End.Row; row++)
                {
                    string ncpf = worksheetDsouPackEchu.Cells[row, 1].Text.Trim();

                    if (!string.IsNullOrEmpty(ncpf))
                    {
                        var cellDdsou = worksheetDsouPackEchu.Cells[row, 3];
                        var cellDfsou = worksheetDsouPackEchu.Cells[row, 4];

                        DateTime? ddsou = ParseDate(cellDdsou);
                        DateTime? dfsou = ParseDate(cellDfsou);
                        dateDsouPackEchu[ncpf] = new DateDsouPackEchuResponse
                        {
                            Ncpf = ncpf,
                            Cpack = worksheetDsouPackEchu.Cells[row, 2].Text.Trim(),
                            Ddsou = ddsou ?? DateTime.MinValue,
                            Dfsou = dfsou ?? DateTime.MinValue,
                        };
                    }
                }
                return dateDsouPackEchu;
            }
            catch (Exception)
            {
                _logger.LogError("Fichier de Comptes Ouvert invalide");
                throw new Exception("Fichier de Comptes Ouvert invalide" );
            }
        }

        public Dictionary<string, List<CompteDebiteRedevCarte>> GetHistCptDebiteRedevCarte(
            ExcelWorksheet worksheetHistCptDebiteRedev
        )
        {
            var result = new Dictionary<string, List<CompteDebiteRedevCarte>>();

            try
            {
                string headerNcp = worksheetHistCptDebiteRedev.Cells[1, 1].Text.Trim();
                string headerMon = worksheetHistCptDebiteRedev.Cells[1, 2].Text.Trim();
                string headerDco = worksheetHistCptDebiteRedev.Cells[1, 3].Text.Trim();
                string headerLib = worksheetHistCptDebiteRedev.Cells[1, 4].Text.Trim();

                if (headerNcp != "NCP" || headerMon != "MON" || headerDco != "DCO")
                {
                    throw new Exception(
                        "Le fichier chargé n'est pas un fichier Compte de redevance valide."
                    );
                }
                for (int row = 2; row <= worksheetHistCptDebiteRedev.Dimension.End.Row; row++)
                {
                    string ncp = worksheetHistCptDebiteRedev.Cells[row, 1].Text.Trim();

                    if (string.IsNullOrWhiteSpace(ncp))
                        continue;

                    long montant = long.Parse(
                        worksheetHistCptDebiteRedev.Cells[row, 2].Text.Trim()
                    );

                    // IMPORTANT :
                    // lire directement la vraie date Excel
                    DateTime datePrelevement = worksheetHistCptDebiteRedev
                        .Cells[row, 3]
                        .GetValue<DateTime>();

                    string lib = worksheetHistCptDebiteRedev.Cells[row, 4].Text.Trim();

                    string mois = datePrelevement.ToString("yyyy-dd");

                    var item = new CompteDebiteRedevCarte
                    {
                        Ncp = ncp,
                        Mon = montant,
                        Dco = mois,
                        Lib = lib,
                    };

                    if (!result.ContainsKey(ncp))
                    {
                        result[ncp] = new List<CompteDebiteRedevCarte>();
                    }

                    result[ncp].Add(item);
                }

                return result;
            }
            catch (Exception)
            { 
                throw new Exception("Fichier de Comptes redevance invalide" );
            }
        }

        public Dictionary<string, PackagesActifsResponse> GetPackagesActifs(
            ExcelWorksheet worksheetPackActif
        )
        {
            var packActif = new Dictionary<string, PackagesActifsResponse>();

            try
            {
                string headerNcpf = worksheetPackActif.Cells[1, 1].Text.Trim();
                string headerCpack = worksheetPackActif.Cells[1, 2].Text.Trim();
                string headerLib = worksheetPackActif.Cells[1, 3].Text.Trim();
                string headerDdsou = worksheetPackActif.Cells[1, 4].Text.Trim();

                if (
                    headerNcpf != "NCPF"
                    || headerCpack != "CPACK"
                    || headerLib != "LIB"
                    || headerDdsou != "DDSOU"
                )
                {
                    throw new Exception(
                        "Le fichier chargé n'est pas un fichier de Packages Actifs valide."
                    );
                }
                for (int row = 2; row <= worksheetPackActif.Dimension!.End.Row; row++)
                {
                    string ncpf = worksheetPackActif.Cells[row, 1].Text!.Trim();

                    if (string.IsNullOrEmpty(ncpf))
                        continue;

                    var cell = worksheetPackActif.Cells[row, 4];

                    DateTime? ddsou = ParseDate(cell);

                    packActif[ncpf] = new PackagesActifsResponse
                    {
                        Ncpf = ncpf,
                        Cpack = worksheetPackActif.Cells[row, 2].Text!.Trim(),
                        Lib = worksheetPackActif.Cells[row, 3].Text!.Trim(),
                        Ddsou = ddsou ?? DateTime.MinValue,
                    };
                }

                return packActif;
            }
            catch (Exception)
            {
                _logger.LogError("Fichier de Packages Actifs invalide");
                throw new Exception("Fichier de Packages Actifs invalide" );
            }
        }

        /// <summary>
        /// Convertit une cellule Excel contenant une date en objet
        /// <see cref="DateTime"/>.
        ///
        /// /// Cette méthode permet de gérer plusieurs formats de dates
        /// provenant des fichiers Excel métiers, notamment :
        /// - dd/MM/yyyy
        /// - MM/dd/yyyy
        /// - dd-MM-yyyy
        /// - MM-dd-yyyy
        /// - ou tout autre format reconnu par .NET.
        ///
        /// Une normalisation est appliquée afin de reconstruire
        /// correctement la date au format métier attendu.
        ///
        /// En cas d’erreur de conversion ou de format invalide,
        /// la méthode retourne <c>null</c> et journalise l’erreur.
        ///
        /// </summary>
        /// <param name="cell">
        /// Cellule Excel contenant la date à convertir.
        /// </param>
        /// <returns>
        /// Une instance de <see cref="DateTime"/> si la conversion réussit ;
        /// sinon <c>null</c>.
        /// </returns>
        // conversion de la ddsou du fichier excel (exple : 12/24/2026 => 24/12/2026)
        public DateTime? ParseDate(ExcelRange cell)
        {
            if (cell == null)
                return null;

            try
            {
                var text = cell.Text!.Trim();
                if (string.IsNullOrEmpty(text))
                    return null;

                // formats: dd/MM/yyyy, MM-dd-yyyy, etc.
                var parts = text.Split('/', '-', '.');

                if (
                    parts.Length == 3
                    && int.TryParse(parts[1], out int p1)
                    && int.TryParse(parts[0], out int p2)
                    && int.TryParse(parts[2], out int p3)
                )
                {
                    // on normalise en dd/MM/yyyy (plus logique métier)
                    int jour = p1;
                    int mois = p2;
                    int annee = p3;

                    return new DateTime(annee, mois, jour);
                }

                // dernier fallback .NET
                if (DateTime.TryParse(text, out var parsed))
                    return parsed;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erreur parsing date: {ex.Message}");
            }

            return null;
        }

        public async Task<IEnumerable<TypeMag>> GetAllTypeMagsAsync()
        {
            return await _typeMagRepository.getAllMag();
        }

        public async Task<TypeMagWithSyntheseDto> GetTypeMagWithSynthese(Guid typeMagId)
        {
            return await _typeMagRepository.GetTypeMagWithSyntheseAsync(typeMagId);
        }

        public ExcelPackage ReadFile(IFormFile file)
        {
            return new ExcelPackage(file.OpenReadStream());
        }

        public byte[] TxtToExcel(List<CarteARegulerDto> carteAReguler)
        {
            try
            {
                using var package = new ExcelPackage();
                using var worksheet = package.Workbook.Worksheets.Add("generate BKMBTI file");

                // creation de l'en-tete du fichier excel
                worksheet.Cells[1, 1].Value = "Numéro Carte";
                worksheet.Cells[1, 2].Value = "Numéro compte";
                worksheet.Cells[1, 3].Value = "Agence";
                worksheet.Cells[1, 4].Value = "Nom Client";
                worksheet.Cells[1, 5].Value = "Code tarif";
                worksheet.Cells[1, 6].Value = "Code carte";

                int row = 2;
                foreach (CarteARegulerDto carte in carteAReguler)
                {
                    worksheet.Cells[row, 1].Value = carte.Carte;
                    worksheet.Cells[row, 2].Value = carte.NumeroCompte;
                    worksheet.Cells[row, 3].Value = carte.CodeAgence;
                    worksheet.Cells[row, 4].Value = carte.NomClient;
                    worksheet.Cells[row, 5].Value = carte.CodeTarif;
                    worksheet.Cells[row, 6].Value = carte.CodeCarte;

                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Mettre en gras et centrer
                var headerRange = worksheet.Cells[1, 1, 1, 6];
                headerRange.Style.Font.Bold = true;
                headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Remplissage vert
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Green);

                // Texte en blanc
                headerRange.Style.Font.Color.SetColor(System.Drawing.Color.White);

                return package.GetAsByteArray();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Erreur lors de la lecture du fichier : {ex.Message}");
                throw new NotImplementedException();
            }
        }
    }
}
