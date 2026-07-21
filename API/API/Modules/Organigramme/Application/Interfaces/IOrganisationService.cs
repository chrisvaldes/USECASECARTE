using Organigramme.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Organigramme.Application.Interfaces
{
    public interface IOrganisationService
    {
        Task<ApiResponse<string>> CreateAsync(OrganisationDto dto);

        Task<ApiResponse<string>> UpdateAsync(Guid id, OrganisationDto dto);

        Task<ApiResponse<string>> DeleteAsync(Guid id);

        Task<ApiResponse<OrganisationDto>> GetByIdAsync(Guid id);

        Task<ApiResponse<List<OrganisationDto>>> GetAllAsync();
    }
}
