
using API.Domain.Entities;
using API.Domain.Entities.Enum;
namespace API.Application.Repository.IRepository
{
    public interface IProfilRepository
    {
        Task<Profil> CreateProfilAsync(Profil profilDto);
        Task<Profil> UpdateProfilAsync(Guid id, Profil profilDto);
        Task<Profil> GetProfilByIdAsync(Guid id);
        Task<IEnumerable<Profil>> GetAllProfilsAsync();
        Task<Profil> GetProfilById(Guid id);
        Task<Profil> DeleteProfilAsync(Guid id);
        Task<List<Profil>> GetProfilsByStatusAsync(EnumStatut status);

        Task<Profil> GetByUseragAsync(string userAg);
    }
}