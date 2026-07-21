using API.Application.DTO;
using API.Application.Services.IServices;
using API.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SYSGES_MAGs.Helpers;

namespace API.Controllers
{
    [ApiController]
    [Route("api")]
    public class NouveauMagController : ControllerBase
    {
        // private readonly IProfilService _profilService;
        private readonly ILogger<NouveauMagController> _logger;

        private readonly IMagProcessingService _magProcessingService;
        private readonly IBkmvtiService _bkmvtiService;
        private readonly MagProcessingHelper _magProcessingHelper;

        public NouveauMagController(
            IMagProcessingService magProcessingService,
            IBkmvtiService bkmvtiService,
            ILogger<NouveauMagController> logger,
            MagProcessingHelper magProcessingHelper
        )
        {
            _magProcessingService = magProcessingService;
            _bkmvtiService = bkmvtiService;
            _magProcessingHelper = magProcessingHelper;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("nouveauMag")]
        [RequestSizeLimit(500 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 500 * 1024 * 1024)]
        public async Task<IActionResult> Nouveau([FromForm] InputModel inputModel)
        {
            //Vérification des paramètres obligatoires
            if (
                inputModel.Apprint == null
                || inputModel.Apprint.Length == 0
                || inputModel.OpenAccount == null
                || inputModel.OpenAccount.Length == 0
                || inputModel.ActiveAccount == null
                || inputModel.ActiveAccount.Length == 0
                || inputModel.DateLastSouPackEchu == null
                || inputModel.DateLastSouPackEchu.Length == 0
                || inputModel.ActivePackage == null
                || inputModel.ActivePackage.Length == 0
                || inputModel.AccountHisDebiteByRedevCard == null
                || inputModel.AccountHisDebiteByRedevCard.Length == 0
                || string.IsNullOrEmpty(inputModel.TypeMag)
                || inputModel.CtxAccount.Length == 0
                || inputModel.StartPeriod == default
                || inputModel.EndPeriod == default
            )
            {
                _logger.LogInformation("Tous les champs sont obligatoires");
                return Ok(
                    ApiResponse<InputModel>.Fail("Tous les champs du formulaire sont requis")
                );
            }
            try
            {
                // Appel du service async correctement
                var result = await _magProcessingService.ProcessTxtExcelFiles(inputModel); 
                if (!result.Success)
                { 
                    return Ok(result);
                }
 
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur inattendue lors du traitement des fichiers MAG");
                return Ok(
                    ApiResponse<bool>.Fail(
                        "Une erreur est survenue lors du traitement des fichiers. " + ex.Message
                    )
                );
            }
        }

        [Authorize]
        [HttpGet("synthese/{id}")]
        public async Task<IActionResult> Synthese(Guid id)
        {
            var result = await _magProcessingHelper.GetTypeMagWithSynthese(id);
            Console.WriteLine($"resultat : {result.Description}");
            // check le type de retour de la synthèse
            return Ok(result);
        }

        [Authorize]
        [HttpGet("typeMag")]
        public async Task<IActionResult> Index()
        {
            IEnumerable<TypeMag> typeMags = await _magProcessingHelper.GetAllTypeMagsAsync();
            return Ok(ApiResponse<IEnumerable<TypeMag>>.SuccessResponse(typeMags, ""));
        }

        [Authorize]
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var result = await _bkmvtiService.DashboardResult();

            if (result == null)
            {
                throw new Exception("DashboardResult() retourne null");
            }

            return Ok(ApiResponse<DashboardSynthese>.SuccessResponse(result, ""));
        }

        [Authorize]
        [HttpGet("bkmvti/Download/{id}")]
        public async Task<IActionResult> DownloadBkmvti(Guid id)
        {
            _logger.LogInformation($"id reçu : {id}");
            if (id == Guid.Empty)
                return Ok(ApiResponse<bool>.Fail("Identifiant du MAG invalide!!!"));

            var bkmvtis = await _bkmvtiService.BkmvtisByMagType(id);

            await _magProcessingHelper.IsDownloadAsync(id);

            if (bkmvtis == null || !bkmvtis.Any())
                return Ok(
                    ApiResponse<bool>.SuccessResponse(
                        true,
                        "Le fichier ne contient aucun manque à gagner à récupérer."
                    )
                );

            var fileBytes = await _magProcessingHelper.GenerateFile(bkmvtis);

            var fileName = $"BKMVTI_{DateTime.Now:yyyyMMddHHmmss}";

            return File(fileBytes, "application/octet-stream", fileName);
        }

        [Authorize]
        [HttpGet("carteAReguler/Download/{id}")]
        public async Task<IActionResult> TelechargerCarteARegulerExcel(Guid id)
        {
            _logger.LogInformation($"id recu : {id}");
            try
            {
                var result = await _bkmvtiService.DashboardResult();

                if (id == Guid.Empty)
                    return Ok(ApiResponse<bool>.Fail("Identifiant du MAG invalide!!!"));

                var carteAReguler = await _bkmvtiService.CarteAReguler(id);
                // Génération du fichier Excel
                byte[] fichier = _magProcessingHelper.TxtToExcel(carteAReguler);

                string nomFichier = $"CarteAReguler_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                return File(
                    fichier,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    nomFichier
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new { Message = "Erreur lors de la génération du fichier", Error = ex.Message }
                );
            }
        }

        [HttpGet("customer-billing")]
        public async Task<IActionResult> GetAllCustomerBilling(
            string ncpf,
            DateTime debut,
            DateTime? fin
        )
        {
            if (string.IsNullOrWhiteSpace(ncpf))
            {
                return Ok(
                    ApiResponse<CustomerBilling>.Fail(
                        "Le paramètre 'ncpf' est requis."
                    )
                );
            }

            var result = await _bkmvtiService.GetAllCustomerBilling(
                new DateTimeOffset(debut),
                ncpf,
                fin.HasValue ? new DateTimeOffset(fin.Value) : (DateTimeOffset?)null
            );
            return Ok(result);
        }
    }
}
