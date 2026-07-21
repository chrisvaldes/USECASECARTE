using API.Domain.Entities;

namespace API.Application.Repository.IRepository
{
    public interface IComptesOuvertRepository
    {
        Task<List<ComptesOuvert>> SaveComptesOuvertAsync(List<ComptesOuvert> comptesOuvert);
        Task<List<ComptesOuvert>> GetByNumerosAsync(List<string> ncps);
        Task<List<ComptesOuvert>> GetByAgeAsync(string age);
        Task<List<ComptesOuvert>> GetAllComptesOuvertsAsync();
    }
}