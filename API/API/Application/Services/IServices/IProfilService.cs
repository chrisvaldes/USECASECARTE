using API.Application.DTO;
using API.Domain.Entities;

namespace API.Application.Services.IServices
{
    public interface IProfilService
    {
        Task<Profil> CreateProfilAsync(Profil profil);

        Task<Profil> UpdateAsync(Guid Id, Profil profil);
        //Task<bool> DeleteAsync(Guid id);
        //Task<ServiceResult<Profil>> GetByIdAsync(Guid id);
        Task<Profil> GetProfilById(Guid id);
        Task<IEnumerable<Profil>> GetAll();
        //public Task<ServiceResult<Profil>> GetByUserag(string userag);
    }
}
