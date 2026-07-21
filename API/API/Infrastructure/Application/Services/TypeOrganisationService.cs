using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Organigramme.Application.DTO;
using Organigramme.Application.Interfaces;
using Organigramme.Domain.Entities;
using Settings.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Application.Services
{
    public class TypeOrganisationService : ITypeOrganisationService
    {
        private readonly ApplicationDbContext _context;

        public TypeOrganisationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<string>> CreateAsync(TypeOrganisationDto dto)
        {
            // Validation des données
            if (string.IsNullOrWhiteSpace(dto.Nom))
                return ApiResponse<string>.Fail("Le nom est obligatoire");

            var existingType = _context.TypeOrganisations.Any(to => to.Nom == dto.Nom);
            if (existingType)
                return ApiResponse<string>.Fail("Ce nom est déjà utilisé.");

            if(dto.ParentId != null)
            {
                var existingParent = _context.TypeOrganisations.Any(to => to.Id == dto.ParentId);
                if(!existingParent)
                    return ApiResponse<string>.Fail("Ce parent n'existe pas.");
            }

            // Insertion des données
            try
            {
                var type = new TypeOrganisation
                {
                    Nom = dto.Nom,
                    ParentId = dto.ParentId,
                };
                _context.TypeOrganisations.Add(type);
                await _context.SaveChangesAsync();

                return ApiResponse<string>.SuccessResponse(type.Id.ToString(), "Type d'organisation enregistrée avec succès!");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail("Une erreur est survenue lors de la sauvegarde du type d'organisation!");
            }
        }

        public async Task<ApiResponse<string>> DeleteAsync(Guid id)
        {
            var type = await _context.TypeOrganisations.FirstOrDefaultAsync(s => s.Uid == id);

            if (type == null)
                return ApiResponse<string>.Fail("Type d'organisation introuvable");

            type.DeletedAt = DateTime.UtcNow;

            _context.SaveChangesAsync();

            return ApiResponse<string>.SuccessResponse(type.Id.ToString(), "Type d'organisation supprimé avec succès!");
        }

        public async Task<ApiResponse<List<TypeOrganisationDto>>> GetAllAsync()
        {
            var types = _context.TypeOrganisations.Include(to => to.Parent).Select(to => new TypeOrganisationDto
            {
                Uid = to.Uid,
                Id = to.Id,
                Nom = to.Nom,
                Parent = to.Parent.Nom,
                ParentId = to.ParentId,
                CreatedAt = to.CreatedAt
            }).OrderByDescending(to => to.CreatedAt).ToList();
            return ApiResponse<List<TypeOrganisationDto>>.SuccessResponse(types);
        }

        public async Task<ApiResponse<TypeOrganisationDto>> GetByIdAsync(Guid id)
        {
            var type = await _context.TypeOrganisations.Include(to => to.Parent).FirstOrDefaultAsync(s => s.Uid == id);

            if (type == null)
                return ApiResponse<TypeOrganisationDto>.Fail("Type d'organisation introuvable");

            return ApiResponse<TypeOrganisationDto>.SuccessResponse(new TypeOrganisationDto
            {
                Id = type.Id,
                Uid = type.Uid,
                Nom = type.Nom,
                Parent = type.Parent?.Nom,
                ParentId = type.ParentId,
                CreatedAt = type.CreatedAt
            });
        }

        public async Task<ApiResponse<string>> UpdateAsync(Guid id, TypeOrganisationDto dto)
        {
            // Validation des données
            if (string.IsNullOrWhiteSpace(dto.Nom))
                return ApiResponse<string>.Fail("Le nom est obligatoire");

            var existingType = _context.TypeOrganisations.Any(to => to.Nom == dto.Nom && to.Uid != id);
            if (existingType)
                return ApiResponse<string>.Fail("Ce nom est déjà utilisé.");

            if (dto.ParentId != null)
            {
                var existingParent = _context.TypeOrganisations.Any(to => to.Id == dto.ParentId);
                if (!existingParent)
                    return ApiResponse<string>.Fail("Ce parent n'existe pas.");
            }

            var type = await _context.TypeOrganisations.FirstOrDefaultAsync(s => s.Uid == id);
            if (type == null)
                return ApiResponse<string>.Fail("Type d'organisation introuvable");

            // Mise à jour des données
            try
            {
                type.Nom = dto.Nom;
                type.ParentId = dto.ParentId;
                type.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return ApiResponse<string>.SuccessResponse(type.Id.ToString(), "Type d'organisation enregistrée avec succès!");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail("Une erreur est survenue lors de la mise à jour du type d'organisation!");
            }
        }
    }
}
