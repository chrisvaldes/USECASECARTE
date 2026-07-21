using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Application.DTO;
using API.Application.Services.IServices;
using API.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api")]
    public class ProfilController : ControllerBase
    {
        private readonly IProfilService _profilService;
        private readonly ILogger<ProfilController> _logger;

        public ProfilController(IProfilService profilService, ILogger<ProfilController> logger)
        {
            _profilService = profilService;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("profil/save")]
        public async Task<IActionResult> SaveProfil([FromBody] Profil profil)
        {
            if (ModelState.IsValid)
            {
                var userProfil = await _profilService.CreateProfilAsync(profil);
                if (userProfil != null)
                {
                    return Ok(ApiResponse<Profil>.SuccessResponse(userProfil, "Profil enregisté."));
                }

                return Ok(ApiResponse<Profil>.Fail("Le profil existe déjà pour cet utilisateur."));
            }
            return Ok(ApiResponse<Profil>.Fail("Tout les champs du formulaire sont requis"));
        }

        [Authorize]
        [HttpGet("profils")]
        public async Task<IActionResult> GetAllProfils()
        {
            var profils = await _profilService.GetAll();
            if (profils != null)
            {
                return Ok(
                    ApiResponse<IEnumerable<Profil>>.SuccessResponse(
                        profils,
                        "Profils récupérés avec succès."
                    )
                );
            }
            return Ok(ApiResponse<IEnumerable<Profil>>.Fail("Aucun profil trouvé."));
        }

        [Authorize]
        [HttpPut("profil/{id}")]
        public async Task<IActionResult> UpdateProfil(Guid id, Profil profil)
        {
            var profilResult = await _profilService.UpdateAsync(id, profil);
            
            if(profilResult != null)
            {
                return Ok(ApiResponse<Profil>.SuccessResponse(profil, "Profil modifié."));
            }
            return Ok(ApiResponse<Profil>.Fail("Aucun profil trouvé."));
            
        }

        [Authorize]
        [HttpGet("profil/{id}")]
        public async Task<IActionResult> GetProfilById(Guid id)
        {
            var profil = await _profilService.GetProfilById(id);
            _logger.LogInformation($"profil {profil.ToString}");
            if (profil != null)
            {
                return Ok(ApiResponse<Profil>.SuccessResponse(profil));
            }
            return Ok(ApiResponse<Profil>.Fail("Aucun profil trouvé."));
        }
    }
}
