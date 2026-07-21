using API.Application.DTO;
using API.Application.Repository.IRepository;
using API.Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace API.Application.Repository
{
    public class BkmvtiRepository : IBkmvtiRepository
    {
        public readonly ApplicationDbContext _dbContext;
        public readonly ILogger<BkmvtiRepository> _logger;

        public BkmvtiRepository(ApplicationDbContext context, ILogger<BkmvtiRepository> logger)
        {
            _dbContext = context;
            _logger = logger;
        }

        public async Task<List<BkmvtiResult>> BkmvtisByMagType(Guid typeMagId)
        {
            return await _dbContext
                .Bkmvtis.Where(x => x.TypeMag == typeMagId)
                .AsNoTracking()
                .Select(x => new BkmvtiResult
                {
                    NumeroCompte = x.NumeroCompte!,
                    CodeAgence = x.CodeAgence!,
                    DatePrelevement = x.DatePrelevement,
                    LibelleCarte = x.LibelleCarte,
                    Montant = x.PrixUnitCarte,
                })
                .OrderBy(x => x.DatePrelevement)
                .ToListAsync();
        }

        public async Task<List<Bkmvti>> SaveBkmvtiAsync(List<Bkmvti> bkmvtis)
        {
            try
            {
                await _dbContext.Bkmvtis.AddRangeAsync(bkmvtis);

                await _dbContext.SaveChangesAsync();

                return bkmvtis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la sauvegarde");

                foreach (var entry in _dbContext.ChangeTracker.Entries())
                {
                    foreach (var prop in entry.Properties)
                    {
                        if (prop.CurrentValue is DateTimeOffset dto)
                        {
                            _logger.LogError(
                                "Entity={Entity}, Property={Property}, Value={Value}, Offset={Offset}",
                                entry.Entity.GetType().Name,
                                prop.Metadata.Name,
                                dto,
                                dto.Offset
                            );
                        }
                    }
                }

                throw;
            }
        }

        // public async Task<List<CarteARegulerDto>> CarteAReguler(Guid typeMagId)
        // {
        //     return await _dbContext
        //         .Database.SqlQuery<CarteARegulerDto>(
        //             $"""
        //                 SELECT DISTINCT
        //                     "DateCreationCarte",
        //                     "Carte",
        //                     "CodeCarte",
        //                     "NumeroCompte",
        //                     "CodeAgence",
        //                     "CodeTarif",
        //                     "NomClient"
        //                 FROM public."Bkmvtis"
        //                 WHERE "Basculer" = TRUE
        //                   AND "TypeMag" = {typeMagId}
        //             """
        //         )
        //         .OrderBy(x => x.NumeroCompte)
        //         .ToListAsync();
        // }

        public async Task<List<CarteARegulerDto>> CarteAReguler(Guid typeMagId)
        {
            return await _dbContext
                .Database.SqlQuery<CarteARegulerDto>(
                    $"""
                    SELECT DISTINCT
                        DateCreationCarte,
                        Carte,
                        CodeCarte,
                        NumeroCompte,
                        CodeAgence,
                        CodeTarif,
                        NomClient
                    FROM Bkmvtis
                    WHERE Basculer = 1
                      AND TypeMag = {typeMagId}
                    """
                )
                .OrderBy(x => x.NumeroCompte)
                .ToListAsync();
        }

        /// <summary>
        /// ///
        /// </summary>
        /// <param name="typeMagId"></param>
        /// <returns></returns>
        public async Task<List<BkmvtiSyntheseDto>> GetSyntheseByTypeMagAsync(Guid typeMagId)
        {
            return await _dbContext
                .Bkmvtis.Where(b => b.TypeMag == typeMagId)
                .GroupBy(b => b.CodeCarte)
                .Select(g => new BkmvtiSyntheseDto
                {
                    CodeCarte = g.Key,
                    NombreClients = g.Count(),
                    DesignationCarte = g.First().DesignationCarte,
                    MontantTotal = g.Sum(x => x.PrixUnitCarte),
                })
                .AsNoTracking()
                .OrderByDescending(x => x.MontantTotal)
                .ToListAsync();
        }

        public async Task<CustomerBilling> GetAllCustomerBilling(
            DateTimeOffset dateDebutFacturation,
            string ncpf,
            DateTimeOffset? dateFinFacturation = null
        )
        {
            try
            {
                CustomerBilling customerSynthese = new();

                var debutUtc = dateDebutFacturation.ToUniversalTime();

                var finUtc = dateFinFacturation?.ToUniversalTime();

                // QUERY COMMUNE
                var query = _dbContext.Bkmvtis.Where(b =>
                    b.NumeroCompte == ncpf
                    && b.DatePrelevement >= debutUtc
                    && (!finUtc.HasValue || b.DatePrelevement <= finUtc.Value)
                );

                // =========================
                // SYNTHESE
                // =========================
                var synthese = await query
                    .GroupBy(b => b.CodeCarte)
                    .Select(g => new BkmvtiSyntheseDto
                    {
                        CodeCarte = g.Key,
                        NombreClients = g.Count(),
                        DesignationCarte = g.First().DesignationCarte,
                        MontantTotal = g.Sum(x => x.PrixUnitCarte),
                    })
                    .OrderByDescending(x => x.MontantTotal)
                    .AsNoTracking()
                    .ToListAsync();

                // =========================
                // DETAIL
                // =========================
                var customerBilling = await query
                    .Select(x => new BkmvtiResult
                    {
                        NumeroCompte = x.NumeroCompte!,
                        CodeCarte = x.CodeCarte!,
                        DatePrelevement = x.DatePrelevement,
                        LibelleCarte = x.LibelleCarte,
                        Montant = x.PrixUnitCarte,
                    })
                    .OrderBy(x => x.CodeCarte)
                    .ThenBy(x => x.DatePrelevement)
                    .AsNoTracking()
                    .ToListAsync();

                customerSynthese.BkmvtiResults = customerBilling;
                customerSynthese.CustomerBillingSynthese = synthese;

                return customerSynthese;
            }
            catch (Exception ex)
            {
                throw new Exception("Erreur lors de la récupération des facturations client", ex);
            }
        }

        public async Task<DashboardSynthese> DashboardResult()
        {
            try
            {
                DashboardSynthese dashboardSynthese = new();

                var synthese = await _dbContext
                    .Bkmvtis.GroupBy(b => b.CodeCarte)
                    .Select(g => new BkmvtiSyntheseDto
                    {
                        CodeCarte = g.Key,
                        NombreClients = g.Count(),
                        DesignationCarte = g.First().DesignationCarte,
                        MontantTotal = g.Sum(x => x.PrixUnitCarte),
                    })
                    .OrderByDescending(x => x.MontantTotal)
                    .AsNoTracking()
                    .ToListAsync();

                var syntheseParMois = await _dbContext
                    .Bkmvtis.AsNoTracking()
                    .Where(x => x.DatePrelevement != null)
                    .GroupBy(x => new { x.DatePrelevement!.Year, x.DatePrelevement.Month })
                    .Select(g => new DashboardResult
                    {
                        Annee = g.Key.Year,
                        Mois = g.Key.Month,
                        Montant = g.Sum(x => x.PrixUnitCarte),
                    })
                    .OrderBy(x => x.Annee)
                    .ThenBy(x => x.Mois)
                    .ToListAsync();

                dashboardSynthese.DashboardResult = syntheseParMois;
                dashboardSynthese.BkmvtiSyntheseDto = synthese;

                return dashboardSynthese;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Erreur synthèse Dashboar.");
                throw new Exception("" + ex.Message);
            }
        }
    }
}
