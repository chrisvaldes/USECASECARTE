using API.Application.DTO;
using API.Application.Repository.IRepository;
using API.Domain.Entities;
using API.Domain.Entities.Enum;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace API.Application.Repository
{
    public class ProfilRepository : IProfilRepository
    {
        protected ApplicationDbContext _dbContext;
        protected ILogger<ProfilRepository> _logger;

        public ProfilRepository(ApplicationDbContext context, ILogger<ProfilRepository> logger)
        {
            _dbContext = context;
            _logger = logger;
        }

        public async Task<Profil> CreateProfilAsync(Profil profil)
        {
            await _dbContext.AddAsync(profil);
            await _dbContext.SaveChangesAsync();

            return profil;
        }

        public async Task<Profil> DeleteProfilAsync(Guid id)
        {
            // Récupérer l'entité par Uid
            var profil = await _dbContext.Profils.FirstOrDefaultAsync(p => p.Uid == id);

            if (profil == null)
            {
                return null; // Rien à supprimer
            }

            profil.IsDeleted = true;

            _dbContext.Profils.Update(profil);
            await _dbContext.SaveChangesAsync();

            return profil;
        }

        public async Task<IEnumerable<Profil>> GetAllProfilsAsync()
        {
            IEnumerable<Profil> profils = await _dbContext
                .Profils.Where(profil => profil.IsDeleted != true)
                .AsNoTracking()
                .ToListAsync();
            return profils;
        }

        public async Task<Profil> GetByUseragAsync(string userAg)
        {
            Profil? profil = await _dbContext.Profils.FirstOrDefaultAsync(x => x.Userag == userAg);

            // Return the found profile or null when not present
            if (profil == null)
            {
                return null;
            }

            return profil;
        }

        public async Task<Profil> GetProfilById(Guid id)
        {
            Profil? profil = await _dbContext.Profils.FirstOrDefaultAsync(p => p.Uid == id);
            if (profil == null)
            {
                return null;
            }
            return profil;
        }

        public async Task<Profil> GetProfilByIdAsync(Guid id)
        {
            Profil? profil = await _dbContext.Profils.FirstOrDefaultAsync(p => p.Uid == id);

            if (profil == null)
            {
                return null; // Retourne null si non trouvé
            }

            _logger.LogInformation("user profile : " + profil.UserName);

            return profil; // Retourne l'objet Profil trouvé
        }

        public Task<List<Profil>> GetProfilsByStatusAsync(EnumStatut status)
        {
            throw new NotImplementedException();
        }

        public async Task<Profil> UpdateProfilAsync(Guid id, Profil profil)
        {
            if (id != profil.Uid)
            {
                return null;
            }

            // Marque l'entité comme modifiée
            _dbContext.Entry(profil).State = EntityState.Modified;

            try
            {
                await _dbContext.SaveChangesAsync();
                Console.WriteLine($"message {profil.Email}");
                return profil;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine($"message {ex.Message}");
                throw new Exception("Erreur de mise-à-jour du profil");
            }
        }
    }
}
