using Authorization.Application.DTO;
using Authorization.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Interfaces
{
    public interface IRoleService
    {
        Task<ApiResponse<string>> CreateAsync(CreateRoleDto dto);
        Task<ApiResponse<string>> UpdateAsync(Guid roleId, UpdateRoleDto dto);
        Task<ApiResponse<string>> DeleteAsync(Guid roleId);
        Task<List<RoleDto>> GetAllAsync();
        Task<ApiResponse<RoleDto>> GetByIdAsync(Guid roleId);
    }
}
