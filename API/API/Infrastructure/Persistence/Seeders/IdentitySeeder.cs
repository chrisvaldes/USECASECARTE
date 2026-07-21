using Authorization.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Entities;

namespace Infrastructure.Persistence.Seeders
{
    public class IdentitySeeder
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ApplicationDbContext _context;
        public IdentitySeeder(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            RoleManager<Role> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task SeedAsync()
        {
            // ========================
            // DÉFINITION DES RÔLES
            // ========================
            var roleDefinitions = new[]
            {
                new { Name = "SUPER_ADMIN", Description = "Accès complet à toutes les fonctionnalités" },
                new { Name = "ADMIN", Description = "Gestion des utilisateurs et profils" },
                new { Name = "GESTIONNAIRE", Description = "Gestion opérationnelle (consultation + modification)" },
                new { Name = "CONSULTANT", Description = "Lecture seule" },
            };

            var createdRoles = new Dictionary<string, Role>();

            foreach (var def in roleDefinitions)
            {
                var role = await _roleManager.FindByNameAsync(def.Name);
                if (role == null)
                {
                    role = new Role
                    {
                        Name = def.Name,
                        NormalizedName = def.Name.ToUpper()
                    };
                    await _roleManager.CreateAsync(role);
                    Console.WriteLine($"Rôle créé : {def.Name}");
                }
                createdRoles[def.Name] = (await _roleManager.FindByNameAsync(def.Name))!;
            }

            // ========================
            // ATTRIBUTION DES PERMISSIONS PAR RÔLE
            // ========================
            var allPermissionCodes = await _context.Set<Permission>()
                .Select(p => p.Code)
                .ToListAsync();

            // SUPER_ADMIN : toutes les permissions
            await AssignFilteredPermissionsToRole(createdRoles["SUPER_ADMIN"], allPermissionCodes);

            // ADMIN : toutes les permissions CRUD sur PROFIL
            var adminPermissions = allPermissionCodes
                .Where(p => p.StartsWith("PROFIL_"))
                .ToList();
            await AssignFilteredPermissionsToRole(createdRoles["ADMIN"], adminPermissions);

            // GESTIONNAIRE : CONSULTER + MODIFIER sur PROFIL
            var gestionnairePermissions = allPermissionCodes
                .Where(p => p == "PROFIL_CONSULTER" || p == "PROFIL_MODIFIER")
                .ToList();
            await AssignFilteredPermissionsToRole(createdRoles["GESTIONNAIRE"], gestionnairePermissions);

            // CONSULTANT : CONSULTER uniquement
            var consultantPermissions = allPermissionCodes
                .Where(p => p == "PROFIL_CONSULTER")
                .ToList();
            await AssignFilteredPermissionsToRole(createdRoles["CONSULTANT"], consultantPermissions);

            // ========================
            // CRÉATION DE L'UTILISATEUR ADMIN
            // ========================
            var adminUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Nom == "Admin");

            if (adminUser == null)
            {
                var newUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@admin.com",
                    Matricule = "admin",
                    Nom = "Admin",
                    Type = "DEFAULT",
                    Prenom = "User",
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(newUser, "Admin@123");

                Console.WriteLine($"USER CREATED: {result.Succeeded}");

                if (!result.Succeeded)
                {
                    throw new Exception("User creation failed: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                adminUser = await _userManager.FindByNameAsync("admin");
            }

            // ========================
            // ATTRIBUTION DU RÔLE SUPER_ADMIN À L'ADMIN
            // ========================
            var userRoles = await _userManager.GetRolesAsync(adminUser);

            if (!userRoles.Contains("SUPER_ADMIN"))
            {
                await _userManager.AddToRoleAsync(adminUser, "SUPER_ADMIN");
                Console.WriteLine("Rôle SUPER_ADMIN attribué à l'utilisateur admin");
            }
        }

        private async Task AssignFilteredPermissionsToRole(Role role, List<string> permissionCodes)
        {
            var existingClaims = await _roleManager.GetClaimsAsync(role);

            var existingValues = existingClaims
                .Where(c => c.Type == "permission")
                .Select(c => c.Value)
                .ToHashSet();

            foreach (var code in permissionCodes)
            {
                if (!existingValues.Contains(code))
                {
                    await _roleManager.AddClaimAsync(role, new Claim("permission", code));
                    Console.WriteLine($"Permission '{code}' ajoutée au rôle '{role.Name}'");
                }
            }
        }
    }
}
