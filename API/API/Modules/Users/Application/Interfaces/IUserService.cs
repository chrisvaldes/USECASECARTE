using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Application.DTO;
using Users.Domain.Entities;

namespace Users.Application.Interfaces
{
    public interface IUserService
    {
        Task<ApplicationUser?> GetByEmailAsync(string email);
        Task<ApiResponse<string>> CreateAsync(CreateUserDto dto);

        Task<ApiResponse<string>> UpdateAsync(Guid id, UpdateUserDto dto);

        Task<ApiResponse<string>> DeleteAsync(Guid id);

        Task<ApiResponse<UserDto>> GetByIdAsync(Guid id);

        Task<ApiResponse<List<UserDto>>> GetAllAsync();
        Task<ApiResponse<PagedResult<UserDto>>> GetUsersPagedAsync(string search, int page, int size);
        Task<ApiResponse<List<SearchUserDto>>> Search(string term);
    }
}
