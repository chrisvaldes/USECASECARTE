// using API.Domain.Entities;
// using Auth.Domain.Entities;
// using Authorization.Domain.Entities;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore;
// using Organigramme.Domain.Entities;
// using Settings.Domain.Entities;
// using Users.Domain.Entities;

// namespace Infrastructure.Persistence
// {
//     public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
//     {
//         public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
//             : base(options) { }

//         public DbSet<Setting> Settings { get; set; }
//         public DbSet<TypeOrganisation> TypeOrganisations { get; set; }
//         public DbSet<Organisation> Organisations { get; set; }
//         public DbSet<LoginHistory> LoginHistories { get; set; }

//         public DbSet<Permission> Permissions { get; set; } 
//         public DbSet<Profil> Profils { get; set; }

//         public DbSet<ComptesOuvert> ComptesOuvert { get; set; }
//         public DbSet<CompteDebiteRedevCarte> ComptesDebiteRedevCartes { get; set; }
//         public DbSet<Bkmvti> Bkmvtis { get; set; }
//         public DbSet<TypeMag> TypeMags { get; set; }

//         protected override void OnModelCreating(ModelBuilder modelBuilder)
//         {
//             base.OnModelCreating(modelBuilder);
//         }
//     }
// }

using API.Domain.Entities;
using Auth.Domain.Entities;
using Authorization.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Organigramme.Domain.Entities;
using Settings.Domain.Entities;
using Users.Domain.Entities;

namespace Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, Role, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Setting> Settings { get; set; }
        public DbSet<TypeOrganisation> TypeOrganisations { get; set; }
        public DbSet<Organisation> Organisations { get; set; }
        public DbSet<LoginHistory> LoginHistories { get; set; }

        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Profil> Profils { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        public DbSet<ComptesOuvert> ComptesOuvert { get; set; }
        public DbSet<CompteDebiteRedevCarte> ComptesDebiteRedevCartes { get; set; }
        public DbSet<Bkmvti> Bkmvtis { get; set; }
        public DbSet<TypeMag> TypeMags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
 
        }
    }
}