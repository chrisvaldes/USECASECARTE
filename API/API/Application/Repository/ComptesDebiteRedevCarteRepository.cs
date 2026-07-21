using API.Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using EFCore.BulkExtensions;
using API.Application.Repository.IRepository;

namespace API.Application.Repository
{
    
    public class ComptesDebiteRedevCarteRepository : IComptesDebiteRedevCarteRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public ComptesDebiteRedevCarteRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Dictionary<string, List<CompteDebiteRedevCarte>>> GetByNumerosAsync(string ncp)
        {
            var result = await _dbContext.ComptesDebiteRedevCartes
                .Where(c => c.Ncp == ncp)
                .AsNoTracking()
                .ToListAsync();

            return result
                .GroupBy(x => x.Ncp)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToList());
        }


        public async Task<List<CompteDebiteRedevCarte>> SaveComptesDebiteAsync(
     List<CompteDebiteRedevCarte> comptes)
        {
            if (comptes == null || !comptes.Any())
                return comptes!;

            await _dbContext.ComptesDebiteRedevCartes.ExecuteDeleteAsync();

            await _dbContext.BulkInsertAsync(comptes);

            return comptes;
        }
    }
}