using API.Domain.Entities;

namespace API.Application.Services.IServices
{
    public interface ICompteDebiteRedevCarteService
    {
        Task SaveAsync(List<CompteDebiteRedevCarte> comptesDebite);
        Task<Dictionary<string, List<CompteDebiteRedevCarte>>> GetByNumerosAsync(string ncp);
    }
}