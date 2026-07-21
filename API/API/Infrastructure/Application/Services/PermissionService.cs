using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Authorization.Application.DTO;
using Authorization.Application.Interfaces;
using Authorization.Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Users.Domain.Entities;

namespace Infrastructure.Application.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PermissionService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager
        )
        {
            _context = context;
            _userManager = userManager;
        }

        //public async Task<List<string>> GetPermissionsByUserId(Guid userId)
        //{
        //    var roles = await _userManager.GetRolesAsync(
        //        await _userManager.FindByIdAsync(userId.ToString())
        //    );

        //    var permissions = await _context.Set<RolePermission>()
        //        .Where(rp => roles.Contains(rp.Role.Name))
        //        .Select(rp => rp.Permission.Code)
        //        .ToListAsync();

        //    return permissions;
        //}

        public async Task<List<string>> GetPermissionsByUserId(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                return new List<string>();

            var roles = await _userManager.GetRolesAsync(user);

            var permissions = new List<string>();

            foreach (var roleName in roles)
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);

                if (role == null)
                    continue;

                var claims = await _context
                    .RoleClaims.Where(c => c.RoleId == role.Id && c.ClaimType == "permission")
                    .Select(c => c.ClaimValue!)
                    .ToListAsync();

                permissions.AddRange(claims);
            }

            return permissions.Distinct().ToList();
        }

        public async Task<List<PermissionTreeDto>> GetAllAsync()
        {
            var permissions = await _context.Set<Permission>().AsNoTracking().ToListAsync();

            var result = permissions
                .Select(p => new PermissionTreeDto
                {
                    Id = p.Id.ToString(),
                    Parent = p.ParentId.HasValue ? p.ParentId.Value.ToString() : "#",
                    Text = p.Name,
                    Code = p.Code,
                })
                .ToList();

            return result;
        }

        public async Task<Permission?> GetPermissionByCode(String code)
        {
            return await _context.Permissions.FirstOrDefaultAsync(permission =>
                permission.Code == code
            );
        }

        public async Task<ApiResponse<Permission>> PostAsync(Permission permission)
        {
            var perm = await GetPermissionByCode(permission.Code);

            if (perm != null)
            {
                return ApiResponse<Permission>.Fail("La permission existe déjà.");
            }
            else
            {
                await _context.AddAsync(permission);
                await _context.SaveChangesAsync();

                return ApiResponse<Permission>.SuccessResponse(
                    permission,
                    "Permission enregistré."
                );
            }
        }
    }
}
