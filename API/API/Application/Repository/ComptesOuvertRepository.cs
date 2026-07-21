using API.Application.Repository.IRepository;
using API.Domain.Entities;
using EFCore.BulkExtensions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace API.Application.Repository
{

    public class ComptesOuvertRepository : IComptesOuvertRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public ComptesOuvertRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<ComptesOuvert>> GetAllComptesOuvertsAsync()
        {
            return await _dbContext.ComptesOuvert
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<ComptesOuvert>> GetByAgeAsync(
            string age)
        {
            return await _dbContext.ComptesOuvert
                .AsNoTracking()
                .Where(c =>
                    c.Age == age &&
                    (
                    c.Ncp.EndsWith("283050") ||
                    c.Ncp.EndsWith("340500")
                    ))
                .OrderByDescending(c => c.Ncp)
                .ToListAsync();
        }

        public async Task<List<ComptesOuvert>> GetByNumerosAsync(List<string> ncps)
        {
            return await _dbContext.ComptesOuvert
                .AsNoTracking()
                .Where(x => ncps.Contains(x.Ncp))
                .ToListAsync();
        }

        //public async Task<List<ComptesOuvert>>  SaveComptesOuvertAsync(List<ComptesOuvert> comptesOuvert)
        //{
        //    await _dbContext.ComptesOuvert.AddRangeAsync(comptesOuvert);
        //    await _dbContext.SaveChangesAsync();
        //    return comptesOuvert;
        //}

        //public async Task<List<ComptesOuvert>> SaveComptesOuvertAsync(List<ComptesOuvert> comptes)
        //{
        //    foreach (var compte in comptes)
        //    {
        //        var existingCompte = await _dbContext.ComptesOuvert
        //            .FirstOrDefaultAsync(x => x.Ncp == compte.Ncp && x.Age == compte.Age && x.Inti == compte.Inti);

        //        if (existingCompte == null)
        //        {
        //            // INSERT
        //            await _dbContext.ComptesOuvert.AddAsync(compte);
        //        }
        //        else
        //        {
        //            // UPDATE
        //            existingCompte.Cfe = compte.Cfe;
        //            existingCompte.Clc = compte.Clc;
        //            existingCompte.Cha = compte.Cha;
        //            existingCompte.Age = compte.Age;
        //            existingCompte.Inti = compte.Inti;
        //            _dbContext.ComptesOuvert.Update(existingCompte);
        //        }
        //    }

        //    await _dbContext.SaveChangesAsync();
        //            return comptes;
        //}



        public async Task<List<ComptesOuvert>> SaveComptesOuvertAsync(
            List<ComptesOuvert> comptes)
        {
            if (comptes == null || !comptes.Any())
                return comptes;

            await _dbContext.ComptesOuvert.ExecuteDeleteAsync();

            await _dbContext.BulkInsertAsync(comptes);

            return comptes;
        }

    }
}