 
using Authorization.Application.DTO;
using Authorization.Domain.Entities;

namespace Authorization.Application.Interfaces
{
    public interface IPermissionService
    {
        Task<List<string>> GetPermissionsByUserId(Guid userId);
        Task<List<PermissionTreeDto>> GetAllAsync();
        Task<ApiResponse<Permission>> PostAsync(Permission permission);
        Task<Permission?> GetPermissionByCode(String code);
    }
}
