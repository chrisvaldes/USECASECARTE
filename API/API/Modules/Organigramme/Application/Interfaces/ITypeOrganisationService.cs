using Organigramme.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Organigramme.Application.Interfaces
{
    public interface ITypeOrganisationService
    {
        Task<ApiResponse<string>> CreateAsync(TypeOrganisationDto dto);

        Task<ApiResponse<string>> UpdateAsync(Guid id, TypeOrganisationDto dto);

        Task<ApiResponse<string>> DeleteAsync(Guid id);

        Task<ApiResponse<TypeOrganisationDto>> GetByIdAsync(Guid id);

        Task<ApiResponse<List<TypeOrganisationDto>>> GetAllAsync();
    }
}
