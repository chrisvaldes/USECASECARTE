using API.Domain.Entities;

namespace API.Application.Services.IServices
{
    public interface IComptesOuvertService
    {
        Task<List<ComptesOuvert>> GetComptesOuvertsAsync(List<string> ncps);
        Task<List<ComptesOuvert>> GetByAgeAsync(string age);
        Task<List<ComptesOuvert>> SaveComptesOuvertAsync(
            List<ComptesOuvert> comptes);
        Task<List<ComptesOuvert>> GetAllComptesOuvertsAsync();
    }
}