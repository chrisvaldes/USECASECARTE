using System.Globalization;
using API.Application.DTO;
using API.Application.Repository.IRepository;
using API.Application.Services.IServices;
using API.Domain.Entities;
using Infrastructure.Persistence;
using SYSGES_MAGs.Helpers;

namespace API.Application.Services
{
    public class MagProcessingService : IMagProcessingService
    {
        private readonly ILogger<MagProcessingService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITypeMagRepository _typeMagRepository;
        private readonly IBkmvtiRepository _bkmvtiRepository;
        private readonly IComptesOuvertRepository _comptesOuvertRepository;
        private readonly IComptesDebiteRedevCarteRepository _comptesDebiteRedevCarteRepository;
        private readonly ApplicationDbContext _dbContext;
        private readonly IEmailService _emailService;

        private readonly MagProcessingHelper _magProcessingHelper;

        // Injection via constructeur
        public MagProcessingService(
            ILogger<MagProcessingService> logger,
            IHttpContextAccessor httpContextAccessor,
            ITypeMagRepository typeMagRepository,
            IBkmvtiRepository kmvtiRepository,
            IServiceScopeFactory serviceScopeFactory,
            ApplicationDbContext context,
            IEmailService emailService,
            IComptesOuvertRepository comptesOuvertRepository,
            IComptesDebiteRedevCarteRepository comptesDebiteRedevCarteRepository,
            MagProcessingHelper magProcessingHelper
        )
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _typeMagRepository = typeMagRepository;
            _bkmvtiRepository = kmvtiRepository;
            _emailService = emailService;
            _dbContext = context;
            _comptesOuvertRepository = comptesOuvertRepository;
            _comptesDebiteRedevCarteRepository = comptesDebiteRedevCarteRepository;
            _magProcessingHelper = magProcessingHelper;
        }

         public async Task<ApiResponse<bool>> ProcessTxtExcelFiles(InputModel inputModel)
        {
            Dictionary<string, ComptesActifsResponse> comptesActifs = new();
            List<ComptesOuvert> comptesOuverts = new();
            Dictionary<string, ComptesCtxResponse> comptesCtx = new();
            Dictionary<string, DateDsouPackEchuResponse> dateDersouPackEchus = new();
            Dictionary<string, List<CompteDebiteRedevCarte>> histCptDebiteRedevCartes = new();
            Dictionary<string, PackagesActifsResponse> packActifs = new();

            List<Bkmvti> bkmvtis = new();
            List<Apprint> apprints = new();
            Dictionary<string, List<Apprint>> clientPlusUneCarte = new();
            DateTimeOffset dateDebutExecution = DateTimeOffset.UtcNow;
            long ligne = 0;

            // Déclaré en dehors du bloc de lecture car réutilisé plus loin (callFuncAsync).
            // Ne PAS mettre en "using var" ici : il doit rester ouvert jusqu'à la fin du traitement.
            StreamReader? reader = null;

            try
            {
                // La période d'étude doit être au minimum 1 mois, et la période de fin d'étude doit être > au début d'étude
                var dureePeriode = _magProcessingHelper.NombreMois(
                    inputModel.StartPeriod,
                    inputModel.EndPeriod
                );

                if ((inputModel.StartPeriod > inputModel.EndPeriod) || (dureePeriode < 1))
                {
                    return ApiResponse<bool>.Fail(
                        "La période de début doit être inférieur à la période de fin, et la différence >= 1"
                    );
                }

                if (inputModel.EndPeriod > DateTime.Today)
                {
                    return ApiResponse<bool>.Fail(
                        "La date de la période de fin d'étude ne peut être supérieur à la date du jour."
                    );
                }

                // Vérifier si le manque à gagner n'a pas encore été récupéré sur la période.
                TypeMag? existTypeMag = await _typeMagRepository.IsTypeMagExist(
                    inputModel.StartPeriod
                );

                if (existTypeMag != null)
                {
                    return ApiResponse<bool>.Fail("Manque à gagner déjà récupéré sur la période.");
                }

                try
                {
                    // lecture du fichier apprint — PAS de "using", reader doit survivre à ce bloc
                    reader = new StreamReader(inputModel.Apprint.OpenReadStream());

                    // lecture des différents fichiers excel : ceux-ci ne sont utilisés
                    // que dans ce bloc, donc "using var" reste correct pour eux.
                    using var compteOuvert = _magProcessingHelper.ReadFile(inputModel.OpenAccount);
                    using var compteActif = _magProcessingHelper.ReadFile(inputModel.ActiveAccount);
                    using var dateDerniereSouPackEchu = _magProcessingHelper.ReadFile(
                        inputModel.DateLastSouPackEchu
                    );
                    using var packageActif = _magProcessingHelper.ReadFile(
                        inputModel.ActivePackage
                    );
                    using var compteCtx = _magProcessingHelper.ReadFile(inputModel.CtxAccount);
                    using var histCptDebiteParRedevCarte = _magProcessingHelper.ReadFile(
                        inputModel.AccountHisDebiteByRedevCard
                    );

                    var worksheetCompteActif = compteActif.Workbook.Worksheets[0];
                    comptesActifs = _magProcessingHelper.GetComptesActifs(worksheetCompteActif);

                    var worksheetCompteCtx = compteCtx.Workbook.Worksheets[0];
                    comptesCtx = _magProcessingHelper.GetComptesCtx(worksheetCompteCtx);

                    var worksheetCompteOuvert = compteOuvert.Workbook.Worksheets[0];
                    comptesOuverts = _magProcessingHelper.GetComptesOuvert(worksheetCompteOuvert);

                    var worksheetDsouPackEchu = dateDerniereSouPackEchu.Workbook.Worksheets[0];
                    dateDersouPackEchus = _magProcessingHelper.GetDsouPackEchu(
                        worksheetDsouPackEchu
                    );

                    var worksheetHistCptDebiteRedev = histCptDebiteParRedevCarte
                        .Workbook
                        .Worksheets[0];
                    histCptDebiteRedevCartes = _magProcessingHelper.GetHistCptDebiteRedevCarte(
                        worksheetHistCptDebiteRedev
                    );

                    var worksheetPackActif = packageActif.Workbook.Worksheets[0];
                    packActifs = _magProcessingHelper.GetPackagesActifs(worksheetPackActif);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Fichier invalide reçu lors du traitement MAG");
                    return ApiResponse<bool>.Fail(ex.Message);
                }

                // Conversion des périodes d'étude
                if (
                    DateTime.TryParse(inputModel.StartPeriod.ToString(), out DateTime startPer)
                    && DateTime.TryParse(inputModel.EndPeriod.ToString(), out DateTime endPer)
                )
                {
                    using var transaction = await _dbContext.Database.BeginTransactionAsync();

                    try
                    {
                        // sauvegarde du type de manque à gagner effectué
                        TypeMag typeMagResult = await _typeMagRepository.SaveTypeMagAsync(
                            new TypeMag
                            {
                                Description =
                                    "cap_"
                                    + startPer.Day.ToString("D2")
                                    + "_"
                                    + startPer.ToString("MMM", new CultureInfo("fr-FR"))
                                    + "_"
                                    + startPer.Year
                                    + "_"
                                    + endPer.Day.ToString("D2")
                                    + "_"
                                    + endPer.ToString("MMM", new CultureInfo("fr-FR"))
                                    + "_"
                                    + endPer.Year,
                                TypeMags = inputModel.TypeMag,
                                Email =
                                    _httpContextAccessor.HttpContext?.User?.Identity?.Name
                                    ?? "unknown",
                                PeriodeDebut = inputModel.StartPeriod,
                                PeriodeFin = inputModel.EndPeriod,
                            }
                        );

                        // ligne d'en-tête
                        string header = await reader.ReadLineAsync() ?? string.Empty;
                        ligne++;

                        List<ComptesOuvert> comptes = new List<ComptesOuvert>();

                        foreach (var c in comptesOuverts)
                        {
                            var compte = new ComptesOuvert
                            {
                                Id = c.Id == Guid.Empty ? Guid.NewGuid() : c.Id,
                                Ncp = c.Ncp,
                                Cfe = c.Cfe,
                                Clc = c.Clc,
                                Cha = c.Cha,
                                Age = c.Age,
                                Inti = c.Inti,
                            };
                            comptes.Add(compte);
                        }

                        List<CompteDebiteRedevCarte> comptesDebite = new();

                        foreach (var item in histCptDebiteRedevCartes)
                        {
                            foreach (var cd in item.Value)
                            {
                                var compteDeb = new CompteDebiteRedevCarte
                                {
                                    Id = Guid.NewGuid(),
                                    Ncp = cd.Ncp,
                                    Mon = cd.Mon,
                                    Dco = cd.Dco,
                                };

                                comptesDebite.Add(compteDeb);
                            }
                        }

                        clientPlusUneCarte = await _magProcessingHelper.callFuncAsync(
                            comptes,
                            reader,
                            ligne,
                            comptesCtx,
                            comptesOuverts,
                            clientPlusUneCarte,
                            comptesDebite
                        );

                        await _magProcessingHelper.MapClientPlusUneCarte(
                            clientPlusUneCarte,
                            dateDersouPackEchus,
                            packActifs,
                            startPer,
                            endPer,
                            typeMagResult,
                            bkmvtis
                        );

                        var comptesOuvertsEntities = comptesOuverts
                            .Select(c => new ComptesOuvert
                            {
                                Id = c.Id == Guid.Empty ? Guid.NewGuid() : c.Id,
                                Ncp = c.Ncp,
                                Cfe = c.Cfe,
                                Clc = c.Clc,
                                Cha = c.Cha,
                                Age = c.Age,
                                Inti = c.Inti,
                            })
                            .ToList();

                        var comptesDebiteEntities = histCptDebiteRedevCartes
                            .Values.SelectMany(x => x)
                            .Select(c => new CompteDebiteRedevCarte
                            {
                                Id = Guid.NewGuid(),
                                Ncp = c.Ncp,
                                Mon = c.Mon,
                                Dco = c.Dco,
                            })
                            .ToList();

                        try
                        {
                            await _bkmvtiRepository.SaveBkmvtiAsync(bkmvtis);
                            await transaction.CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError(ex, "Erreur sauvegarde");
                            throw;
                        }
                    }
                    catch (Exception ex)
                    {
                        return ApiResponse<bool>.Fail("Erreur : " + ex.Message);
                    }
                }

                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Manque à gagner traité avec succès!!!"
                );
            }
            catch (Exception ex)
            {
                _logger.LogInformation("message erreur de traitement. cause : " + ex.Message);
                return ApiResponse<bool>.Fail(
                    "message erreur de traitement. cause : " + ex.Message
                );
            }
            finally
            {
                // reader n'était pas en "using" car réutilisé plus bas dans la méthode ;
                // on le libère explicitement ici, quel que soit le chemin de sortie.
                reader?.Dispose();
            }
        }
    }
}
