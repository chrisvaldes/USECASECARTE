using Settings.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settings.Application.Interfaces
{
    public interface ISettingService
    {
        Task<string?> GetValueAsync(string key);
        Task<T?> GetAsync<T>(string key);
        Task<ApiResponse<string>> CreateAsync(SettingDto dto);

        Task<ApiResponse<string>> UpdateAsync(Guid id, SettingDto dto);

        Task<ApiResponse<string>> DeleteAsync(Guid id);

        Task<ApiResponse<SettingDto>> GetByIdAsync(Guid id);

        Task<ApiResponse<List<SettingDto>>> GetAllAsync();
    }
}
