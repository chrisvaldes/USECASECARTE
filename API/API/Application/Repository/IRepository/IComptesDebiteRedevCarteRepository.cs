using API.Domain.Entities;

namespace API.Application.Repository.IRepository
{
    public interface IComptesDebiteRedevCarteRepository
    {
        Task<List<CompteDebiteRedevCarte>> SaveComptesDebiteAsync(List<CompteDebiteRedevCarte> comptesDebite);
        Task<Dictionary<string, List<CompteDebiteRedevCarte>>> GetByNumerosAsync(string ncp);

    }
}