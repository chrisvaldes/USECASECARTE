using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Organigramme.Application.DTO;
using Organigramme.Application.Interfaces;
using Organigramme.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Application.Services
{
    public class OrganisationService : IOrganisationService
    {
        private readonly ApplicationDbContext _context;

        public OrganisationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<string>> CreateAsync(OrganisationDto dto)
        {
            // Validation des données
            if (string.IsNullOrWhiteSpace(dto.Nom))
                return ApiResponse<string>.Fail("Le nom est obligatoire");
            if (string.IsNullOrWhiteSpace(dto.Code))
                return ApiResponse<string>.Fail("Le code est obligatoire");
            if (dto.ResponsableId == null)
                return ApiResponse<string>.Fail("Le responsable de l'organisation est obligatoire");

            if (string.IsNullOrWhiteSpace(dto.TypeId) || !Guid.TryParse(dto.TypeId, out var typeGuid))
                return ApiResponse<string>.Fail("Le type de l'organisation est obligatoire");

            var type = _context.TypeOrganisations.FirstOrDefault(t => t.Uid == typeGuid);
            if (type == null)
                return ApiResponse<string>.Fail("Le type de l'organisation est introuvable");

            var existingCode = _context.Organisations.Any(to => to.Code == dto.Code);
            if (existingCode)
                return ApiResponse<string>.Fail("Ce code est déjà utilisé.");

            if (!string.IsNullOrEmpty(dto.ParentId) || !string.IsNullOrWhiteSpace(dto.ParentId))
            {
                var existingParent = _context.Organisations.Any(to => to.Uid == Guid.Parse(dto.ParentId));
                if (!existingParent)
                    return ApiResponse<string>.Fail("Ce parent n'existe pas.");
            }

            // Insertion des données
            try
            {
                var parent = string.IsNullOrEmpty(dto.ParentId) ? null : _context.Organisations.FirstOrDefault(to => to.Uid == Guid.Parse(dto.ParentId));
                
                var organisation = new Organisation
                {
                    Code = dto.Code,
                    Nom = dto.Nom,
                    ResponsableId = dto.ResponsableId,
                    ParentId = string.IsNullOrEmpty(dto.ParentId) ? null : parent.Id,
                    TypeId = type.Id,
                };
                _context.Organisations.Add(organisation);
                await _context.SaveChangesAsync();

                return ApiResponse<string>.SuccessResponse(organisation.Id.ToString(), "Organisation enregistrée avec succès!");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail(ex.ToString());
            }
        }

        public async Task<ApiResponse<string>> DeleteAsync(Guid id)
        {
            var organisation = await _context.Organisations.FirstOrDefaultAsync(s => s.Uid == id);

            if (organisation == null)
                return ApiResponse<string>.Fail("Type d'organisation introuvable");

            organisation.DeletedAt = DateTime.UtcNow;

            _context.SaveChangesAsync();

            return ApiResponse<string>.SuccessResponse(organisation.Id.ToString(), "Organisation supprimé avec succès!");
        }

        public async Task<ApiResponse<List<OrganisationDto>>> GetAllAsync()
        {
            var organisations = _context.Organisations
                .Include(o => o.Parent)
                .Include(o => o.Responsable)
                .Include(o => o.TypeOrganisation)
                .Select(to => new OrganisationDto
                {
                    Uid = to.Uid,
                    Id = to.Id,
                    Nom = to.Nom,
                    Code = to.Code,
                    ResponsableId = to.ResponsableId,
                    Responsable = to.Responsable.FullName,
                    Parent = to.Parent.Nom,
                    TypeId = to.TypeOrganisation.Uid.ToString(),
                    ParentId = to.ParentId == null ? null : to.Parent.Uid.ToString(),
                    CreatedAt = to.CreatedAt
                }).ToList();
            return ApiResponse<List<OrganisationDto>>.SuccessResponse(organisations);
        }

        public async Task<ApiResponse<OrganisationDto>> GetByIdAsync(Guid id)
        {
            var organisation = await _context.Organisations
                .Include(to => to.Parent)
                .Include(to => to.Responsable)
                .Include(to => to.TypeOrganisation)
                .FirstOrDefaultAsync(s => s.Uid == id);

            if (organisation == null)
                return ApiResponse<OrganisationDto>.Fail("Organisation introuvable");

            return ApiResponse<OrganisationDto>.SuccessResponse(new OrganisationDto
            {
                Id = organisation.Id,
                Uid = organisation.Uid,
                Nom = organisation.Nom,
                Code = organisation.Code,
                ResponsableId = organisation.ResponsableId,
                Responsable = organisation.Responsable.FullName,
                TypeId = organisation.TypeOrganisation.Uid.ToString(),
                ParentId = organisation.Parent == null ? null : organisation.Parent.Uid.ToString(),
                Parent = organisation.Parent == null ? null : organisation.Parent.Nom,
                CreatedAt = organisation.CreatedAt
            });
        }

        public async Task<ApiResponse<string>> UpdateAsync(Guid id, OrganisationDto dto)
        {
            // Validation des données
            if (string.IsNullOrWhiteSpace(dto.Nom))
                return ApiResponse<string>.Fail("Le nom est obligatoire");
            if (string.IsNullOrWhiteSpace(dto.Code))
                return ApiResponse<string>.Fail("Le code est obligatoire");
            if (dto.ResponsableId == null)
                return ApiResponse<string>.Fail("Le responsable de l'organisation est obligatoire");

            var existingCode = _context.Organisations.Any(o => o.Code == dto.Code && o.Uid != id);
            if (existingCode)
                return ApiResponse<string>.Fail("Ce code est déjà utilisé.");

            var type = _context.TypeOrganisations.FirstOrDefault(t => t.Uid == Guid.Parse(dto.TypeId));
            if (type == null)
                return ApiResponse<string>.Fail("Le type de l'organisation est obligatoire");

            if (!string.IsNullOrEmpty(dto.ParentId) && Guid.TryParse(dto.ParentId, out var parentGuid))
            {
                var existingParent = _context.Organisations.Any(to => to.Uid == parentGuid);
                if (!existingParent)
                    return ApiResponse<string>.Fail("Ce parent n'existe pas.");
            }

            var organisation = _context.Organisations.FirstOrDefault(o => o.Uid == id);
            if (organisation == null)
                return ApiResponse<string>.Fail("Organisation introuvable");

            // Mise à jour des données
            try
            {
                var parent = string.IsNullOrEmpty(dto.ParentId) ? null : _context.Organisations.FirstOrDefault(to => to.Uid == Guid.Parse(dto.ParentId));

                organisation.Code = dto.Code;
                organisation.Nom = dto.Nom;
                organisation.ParentId = string.IsNullOrEmpty(dto.ParentId) ? null : parent.Id;
                organisation.ResponsableId = dto.ResponsableId;
                organisation.TypeId = type.Id;
                organisation.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse<string>.SuccessResponse(organisation.Id.ToString(), "Organisation enregistrée avec succès!");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail("Une erreur est survenue lors de la mise à jour de l'organisation!");
            }
        }
    }
}
