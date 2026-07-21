using System.Security.Claims;
using Authorization.Application.DTO;
using Authorization.Application.Interfaces;
using Authorization.Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Users.Domain.Entities;

namespace Infrastructure.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleService(
            ApplicationDbContext context,
            RoleManager<Role> roleManager,
            UserManager<ApplicationUser> userManager
        )
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<ApiResponse<string>> CreateAsync(CreateRoleDto dto)
        {
            // Validation basique
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return ApiResponse<string>.Fail("Le nom du rôle est obligatoire");
            }

            // 🔹 Vérifier si le rôle existe
            var existingRole = await _roleManager.FindByNameAsync(dto.Name);
            if (existingRole != null)
            {
                return ApiResponse<string>.Fail("Ce rôle existe déjà");
            }

            //var role = new IdentityRole(dto.Name);
            var role = new Role { Name = dto.Name };
            var result = await _roleManager.CreateAsync(role);

            // Gestion des erreurs Identity
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();

                return ApiResponse<string>.Fail("Erreur lors de la création du rôle", errors);
            }

            // Ajouter les permissions
            var permissionErrors = new List<string>();

            foreach (var permissionId in dto.Permissions)
            {
                var permission = await _context.Set<Permission>().FindAsync(permissionId);

                if (permission == null)
                {
                    permissionErrors.Add($"Permission avec ID {permissionId} introuvable");
                    continue;
                }

                var claimResult = await _roleManager.AddClaimAsync(
                    role,
                    new Claim("Permission", permission.Code)
                );

                if (!claimResult.Succeeded)
                {
                    permissionErrors.AddRange(claimResult.Errors.Select(e => e.Description));
                }
            }

            // Si erreurs sur permissions
            if (permissionErrors.Any())
            {
                return ApiResponse<string>.Fail(
                    "Rôle créé mais certaines permissions ont échoué",
                    permissionErrors
                );
            }

            return ApiResponse<string>.SuccessResponse(role.Id.ToString(), "Rôle créé avec succès");
        }

        public async Task<ApiResponse<string>> UpdateAsync(Guid roleId, UpdateRoleDto dto)
        {
            // Validation
            if (roleId == Guid.Empty)
            {
                return ApiResponse<string>.Fail("Identifiant du rôle invalide");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return ApiResponse<string>.Fail("Le nom du rôle est obligatoire");
            }

            // Récupération du rôle
            var role = await _roleManager.FindByIdAsync(roleId.ToString());

            if (role == null)
            {
                return ApiResponse<string>.Fail("Rôle introuvable");
            }

            // Vérifier si un autre rôle a le même nom
            var existingRole = await _roleManager.FindByNameAsync(dto.Name);

            if (existingRole != null && existingRole.Id != roleId)
            {
                return ApiResponse<string>.Fail("Un autre rôle avec ce nom existe déjà");
            }

            // Gestion des permissions
            try
            {
                // Supprimer anciennes permissions
                var currentClaims = await _roleManager.GetClaimsAsync(role);

                foreach (var claim in currentClaims.Where(c => c.Type == "Permission"))
                {
                    var results = await _roleManager.RemoveClaimAsync(role, claim);
                    if (!results.Succeeded)
                        throw new Exception("Erreur suppression claim");
                }

                // Ajouter nouvelles permissions
                foreach (var permissionId in dto.Permissions)
                {
                    var permission = await _context.Set<Permission>().FindAsync(permissionId);
                    if (permission != null)
                    {
                        var results = await _roleManager.AddClaimAsync(
                            role,
                            new Claim("Permission", permission.Code)
                        );

                        if (!results.Succeeded)
                            throw new Exception("Erreur ajout claim");
                    }
                }

                // Mise à jour du nom
                role.Name = dto.Name;
                role.NormalizedName = dto.Name.ToUpper();
                var result = await _roleManager.UpdateAsync(role);

                return ApiResponse<string>.SuccessResponse(
                    role.Id.ToString(),
                    "Rôle mis à jour avec succès"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail(
                    "Erreur lors de la mise à jour du rôle",
                    new List<string> { ex.Message }
                );
            }
        }

        public async Task<ApiResponse<string>> DeleteAsync(Guid roleId)
        {
            // Validation
            if (roleId == Guid.Empty)
            {
                return ApiResponse<string>.Fail("Identifiant du rôle invalide");
            }

            try
            {
                // Récupérer le rôle
                var role = await _roleManager.FindByIdAsync(roleId.ToString());

                if (role == null)
                {
                    return ApiResponse<string>.Fail("Rôle introuvable");
                }

                // Vérifier si le rôle est utilisé
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);

                if (usersInRole.Any())
                {
                    return ApiResponse<string>.Fail(
                        "Impossible de supprimer ce rôle car il est assigné à des utilisateurs"
                    );
                }

                // 🔹 Supprimer les claims (permissions)
                var claims = await _roleManager.GetClaimsAsync(role);

                foreach (var claim in claims)
                {
                    var result = await _roleManager.RemoveClaimAsync(role, claim);

                    if (!result.Succeeded)
                    {
                        return ApiResponse<string>.Fail(
                            "Erreur lors de la suppression des permissions",
                            result.Errors.Select(e => e.Description).ToList()
                        );
                    }
                }

                // 🔹 Supprimer le rôle
                var deleteResult = await _roleManager.DeleteAsync(role);

                if (!deleteResult.Succeeded)
                {
                    return ApiResponse<string>.Fail(
                        "Erreur lors de la suppression du rôle",
                        deleteResult.Errors.Select(e => e.Description).ToList()
                    );
                }

                return ApiResponse<string>.SuccessResponse(
                    role.Id.ToString(),
                    "Rôle supprimé avec succès"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail(
                    "Erreur lors de la suppression du rôle",
                    new List<string> { ex.Message }
                );
            }
        }

        public async Task<List<RoleDto>> GetAllAsync()
        {
            var roles = _roleManager.Roles.ToList();
            var rolePermissions = new List<RoleDto>();
            var permissionsAll = _context.Set<Permission>().ToList();
            foreach (var role in roles)
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                var claims_value = claims
                    .Where(c => c.Type == "Permission")
                    .Select(c => c.Value)
                    .ToList();
                var permissions = permissionsAll
                    .Where(p => p.ParentId != null && claims_value.Contains(p.Code))
                    .Select(p => p.Name)
                    .ToList();
                rolePermissions.Add(
                    new RoleDto
                    {
                        Id = role.Id.ToString(),
                        Name = role.Name,
                        Permissions = permissions,
                    }
                );
            }

            return rolePermissions;
        }

        public async Task<ApiResponse<RoleDto>> GetByIdAsync(Guid roleId)
        {
            // Validation
            if (roleId == Guid.Empty)
            {
                return ApiResponse<RoleDto>.Fail("Identifiant du rôle invalide");
            }

            try
            {
                var role = await _roleManager.FindByIdAsync(roleId.ToString());

                if (role == null)
                {
                    return ApiResponse<RoleDto>.Fail("Rôle introuvable");
                }

                // Récupérer les claims (permissions)
                var claims = await _roleManager.GetClaimsAsync(role);

                var permissions = claims
                    .Where(c => c.Type == "Permission")
                    .Select(c => c.Value)
                    .ToList();

                var result = new RoleDto
                {
                    Id = role.Id.ToString(),
                    Name = role.Name!,
                    Permissions = permissions,
                };

                return ApiResponse<RoleDto>.SuccessResponse(result, "Rôle récupéré avec succès");
            }
            catch (Exception ex)
            {
                return ApiResponse<RoleDto>.Fail(
                    "Erreur lors de la récupération du rôle",
                    new List<string> { ex.Message }
                );
            }
        }
    }
}
