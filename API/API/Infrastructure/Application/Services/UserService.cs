using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Authorization.Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Users.Application.DTO;
using Users.Application.Interfaces;
using Users.Domain.Entities;

namespace Infrastructure.Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ApplicationDbContext _context;

        public UserService(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            RoleManager<Role> roleManager
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<ApplicationUser?> GetByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<ApiResponse<string>> CreateAsync(CreateUserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return ApiResponse<string>.Fail("Email obligatoire");

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return ApiResponse<string>.Fail("Email déjà utilisé");

            if (string.IsNullOrWhiteSpace(dto.Matricule))
                return ApiResponse<string>.Fail("Matricule obligatoire");

            var existingUserMatricule = _userManager.Users.Any(u => u.Matricule == dto.Matricule);
            if (existingUserMatricule)
                return ApiResponse<string>.Fail("Matricule déjà utilisé");

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = dto.Matricule,
                    Email = dto.Email,
                    Matricule = dto.Matricule,
                    Nom = dto.Nom,
                    Prenom = dto.Prenom,
                    Type = dto.Type,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                };

                var result = await _userManager.CreateAsync(user, dto.Password);

                user = await _userManager.Users.FirstOrDefaultAsync(u =>
                    u.Matricule == dto.Matricule && u.Email == dto.Email
                );

                if (!result.Succeeded)
                    return ApiResponse<string>.Fail(
                        "Erreur création utilisateur",
                        result.Errors.Select(e => e.Description).ToList()
                    );
 
                // Assign roles
                if (dto.RoleIds.Any())
                {
                    var roles = _roleManager
                        .Roles.Where(r => dto.RoleIds.Contains(r.Id.ToString()))
                        .Select(r => r.Name)
                        .ToList();

                    await _userManager.AddToRolesAsync(user, roles!);
                }

                // Valider la transaction
                await transaction.CommitAsync();

                return ApiResponse<string>.SuccessResponse(user.Id.ToString(), "Utilisateur créé");
            }
        }

        public async Task<ApiResponse<string>> UpdateAsync(Guid id, UpdateUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
                return ApiResponse<string>.Fail("Utilisateur introuvable");

            user.Matricule = dto.Matricule;
            user.Nom = dto.Nom;
            user.Prenom = dto.Prenom;
            user.Email = dto.Email;
            user.UserName = dto.Email;
            user.Type = dto.Type;
            user.IsActive = dto.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return ApiResponse<string>.Fail("Erreur mise à jour");

            // Roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            var newRoles = _roleManager
                .Roles.Where(r => dto.Roles.Contains(r.Id.ToString()))
                .Select(r => r.Name)
                .ToList();

            await _userManager.AddToRolesAsync(user, newRoles!);

            return ApiResponse<string>.SuccessResponse(
                user.Id.ToString(),
                "Utilisateur mis à jour"
            );
        }

        public async Task<ApiResponse<UserDto>> GetByIdAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
                return ApiResponse<UserDto>.Fail("Utilisateur introuvable");

            var roleNames = await _userManager.GetRolesAsync(user);

            var roles = _roleManager.Roles.Where(r => roleNames.Contains(r.Name)).ToList();

            return ApiResponse<UserDto>.SuccessResponse(
                new UserDto
                {
                    Id = user.Id,
                    Matricule = user.Matricule,
                    Nom = user.Nom,
                    Prenom = user.Prenom,
                    Email = user.Email,
                    Type = user.Type,
                    IsActive = user.IsActive,
                    Roles = roles.Select(r => r.Name).ToList(),
                    RoleIds = roles.Select(r => r.Id.ToString()).ToList(),
                }
            );
        }

        public async Task<ApiResponse<List<UserDto>>> GetAllAsync()
        {
            var users = _userManager.Users.ToList();

            var result = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                result.Add(
                    new UserDto
                    {
                        Id = user.Id,
                        Matricule = user.Matricule,
                        Nom = user.Nom,
                        Prenom = user.Prenom,
                        Email = user.Email,
                        Type = user.Type,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt,
                        Roles = roles.ToList(),
                    }
                );
            }

            return ApiResponse<List<UserDto>>.SuccessResponse(result);
        }

        public async Task<ApiResponse<string>> DeleteAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
                return ApiResponse<string>.Fail("Utilisateur introuvable");

            user.IsActive = false;
            user.DeletedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            return ApiResponse<string>.SuccessResponse(user.Id.ToString(), "Utilisateur désactivé");
        }

        public async Task<ApiResponse<PagedResult<UserDto>>> GetUsersPagedAsync(
            string search,
            int page,
            int size
        )
        {
            try
            {
                if (page <= 0)
                    page = 1;
                if (size <= 0)
                    size = 10;

                var query = _userManager.Users.AsQueryable();

                // Search
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();

                    query = query.Where(u =>
                        u.Nom.ToLower().Contains(search)
                        || u.Prenom!.ToLower().Contains(search)
                        || u.Email!.ToLower().Contains(search)
                        || u.Matricule.ToLower().Contains(search)
                    );
                }

                var totalCount = await query.CountAsync();

                var users = await query
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((page - 1) * size)
                    .Take(size)
                    .ToListAsync();

                var result = new List<UserDto>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    result.Add(
                        new UserDto
                        {
                            Id = user.Id,
                            Matricule = user.Matricule,
                            Nom = user.Nom,
                            Prenom = user.Prenom,
                            Email = user.Email,
                            Type = user.Type,
                            IsActive = user.IsActive,
                            Roles = roles.ToList(),
                        }
                    );
                }

                var paged = new PagedResult<UserDto>
                {
                    Items = result,
                    TotalCount = totalCount,
                    Page = page,
                    Size = size,
                };

                return ApiResponse<PagedResult<UserDto>>.SuccessResponse(
                    paged,
                    "Liste des utilisateurs récupérée avec succès"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResult<UserDto>>.Fail(
                    "Erreur lors de la récupération des utilisateurs",
                    new List<string> { ex.Message }
                );
            }
        }

        public async Task<ApiResponse<List<SearchUserDto>>> Search(string term)
        {
            var result = await _context
                .Users.Where(user =>
                    EF.Functions.ILike(user.Nom, $"%{term}%")
                    || EF.Functions.ILike(user.Prenom, $"%{term}%")
                    || EF.Functions.ILike(user.Matricule, $"%{term}%")
                )
                .Select(user => new SearchUserDto
                {
                    id = user.Id,
                    text = user.Nom + " " + user.Prenom,
                })
                .Take(20)
                .ToListAsync();

            return ApiResponse<List<SearchUserDto>>.SuccessResponse(result);
        }
    }
}
