using API.Application.DTO;
using API.Application.Repository.IRepository;
using API.Application.Services.IServices;
using API.Domain.Entities;
using API.Domain.Entities.Enum;

namespace API.Application.Service.ProfilService
{
    public class ProfilService : IProfilService
    {
        private readonly IProfilRepository _profilRepository;
        private readonly ILogger<ProfilService> _logger;

        public ProfilService(IProfilRepository profilRepository, ILogger<ProfilService> logger)
        {
            _profilRepository = profilRepository;
            _logger = logger;
        }

        public async Task<Profil> CreateProfilAsync(Profil profil)
        {
            try
            {
                var profilResult = await _profilRepository.GetByUseragAsync(profil.Userag!);

                if (profilResult != null)
                {
                    _logger.LogWarning("Profil already exists.");
                    return null;
                }

                _logger.LogInformation($"Creating new profile for userag: {profil?.Userag}");
                var created = await _profilRepository.CreateProfilAsync(profil);

                return created;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Une erreur c'est produite lors de la sauvegarde." + ex.Message
                );
            }
        }

        public Task<IEnumerable<Profil>> GetAll()
        {
            try
            {
                var profils = _profilRepository.GetAllProfilsAsync();
                if (profils == null)
                {
                    _logger.LogWarning("No profiles found.");
                    return null;
                }
                return profils;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Une erreur c'est produite lors de la récupération des profils." + ex.Message
                );
            }
        }

        public Task<Profil> GetProfilById(Guid id)
        {
            try
            {
                var profil = _profilRepository.GetProfilById(id);
                if (profil == null)
                {
                    _logger.LogWarning("No profiles found.");
                    return null;
                }
                return profil;
            }
            catch (Exception ex)
            {
                throw new Exception("Erreur : " + ex.Message);
            }
        }

        public async Task<Profil> UpdateAsync(Guid id, Profil profil)
        {
            try
            {
                Profil profilResult = await _profilRepository.UpdateProfilAsync(id, profil);
                if(profilResult == null)
                {
                    return null;
                }
                return profilResult;
            }catch(Exception ex)
            {
                throw new Exception($"Le profil n'existe pas {ex.Message}");
            }
        }
    }
}
