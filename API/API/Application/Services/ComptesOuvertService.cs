using API.Application.Repository.IRepository;
using API.Application.Services.IServices;
using API.Domain.Entities;

namespace API.Application.Services
{
    public class ComptesOuvertService : IComptesOuvertService
    {
        private readonly IComptesOuvertRepository _comptesOuvertRepository;

        public ComptesOuvertService(IComptesOuvertRepository comptesOuvertRepository)
        {
            _comptesOuvertRepository = comptesOuvertRepository;
        }

        public Task<List<ComptesOuvert>> GetByAgeAsync(string age)
        {
            return _comptesOuvertRepository.GetByAgeAsync(age);
        }

        public Task<List<ComptesOuvert>> GetComptesOuvertsAsync(List<string> ncps)
        {
            return _comptesOuvertRepository
                .GetByNumerosAsync(ncps);
        }

        public async Task<List<ComptesOuvert>> SaveComptesOuvertAsync(
            List<ComptesOuvert> comptes)
        {
            return await _comptesOuvertRepository.SaveComptesOuvertAsync(comptes);
        }

        public async Task<List<ComptesOuvert>> GetAllComptesOuvertsAsync()
        {
            return await _comptesOuvertRepository.GetAllComptesOuvertsAsync();
        }

    }
}