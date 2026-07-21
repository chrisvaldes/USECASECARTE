using API.Application.Repository.IRepository;
using API.Application.Services.IServices;
using API.Domain.Entities;

namespace API.Application.Services
{
     public class CompteDebiteRedevCarteService:ICompteDebiteRedevCarteService
 {
     private readonly IComptesDebiteRedevCarteRepository _repository;
     public CompteDebiteRedevCarteService(IComptesDebiteRedevCarteRepository repository)
     {
         _repository = repository;
     }  

     public Task SaveAsync(List<CompteDebiteRedevCarte> comptesDebite)
     {
         return _repository.SaveComptesDebiteAsync(comptesDebite);
     }

     public async Task<Dictionary<string, List<CompteDebiteRedevCarte>>> GetByNumerosAsync(string ncp)
     {
         return await _repository.GetByNumerosAsync(ncp);
     }
 }
}