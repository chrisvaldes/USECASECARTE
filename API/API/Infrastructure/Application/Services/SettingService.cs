using Infrastructure.Persistence;
//using Microsoft.AspNet.Identity;
using Microsoft.EntityFrameworkCore;
using Settings.Application.DTO;
using Settings.Application.Interfaces;
using Settings.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Application.Services
{
    public class SettingService : ISettingService
    {
        private readonly ApplicationDbContext _context;

        public SettingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string?> GetValueAsync(string key)
        {
            return await _context.Set<Setting>()
                .Where(x => x.Key == key)
                .Select(x => x.Value)
                .FirstOrDefaultAsync();
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await GetValueAsync(key);

            if (value == null) return default;

            return JsonSerializer.Deserialize<T>(value);
        }

        public async Task<ApiResponse<List<SettingDto>>> GetAllAsync()
        {
            var settings = _context.Settings
                .Select(u => new SettingDto
                {
                    Id = u.Id,
                    Uid = u.Uid,
                    Key = u.Key,
                    Value = u.Value,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                })
                .ToList();

            return ApiResponse<List<SettingDto>>.SuccessResponse(settings);
        }

        public async Task<ApiResponse<SettingDto>> GetByIdAsync(Guid id)
        {
            var setting = await _context.Settings.FirstOrDefaultAsync(s => s.Uid == id);

            if (setting == null)
                return ApiResponse<SettingDto>.Fail("Paramétrage introuvable");
                        
            return ApiResponse<SettingDto>.SuccessResponse(new SettingDto
            {
                Id = setting.Id,
                Uid=setting.Uid,
                Key = setting.Key,
                Value = setting.Value,
                CreatedAt = setting.CreatedAt,
                UpdatedAt = setting.UpdatedAt
            });
        }


        public async Task<ApiResponse<string>> DeleteAsync(Guid id)
        {
            var setting = await _context.Settings.FirstOrDefaultAsync(s => s.Uid == id);

            if (setting == null)
                return ApiResponse<string>.Fail("Paramétrage introuvable");

            setting.DeletedAt = DateTime.UtcNow;

            _context.Settings.Update(setting);

            return ApiResponse<string>.SuccessResponse(setting.Id.ToString(), "Paramétrage désactivé");
        }

        public async Task<ApiResponse<string>> CreateAsync(SettingDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Key))
                return ApiResponse<string>.Fail("Clé obligatoire");

            var existingSetting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == dto.Key);
            if (existingSetting != null)
                return ApiResponse<string>.Fail("Clé déjà utilisé");
            try
            {
                var setting = new Setting
                {
                    Key = dto.Key,
                    Value = dto.Value,
                };
                _context.Settings.Add(setting);
                await _context.SaveChangesAsync();

                return ApiResponse<string>.SuccessResponse(setting.Id.ToString(), "Paramétrage créé");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail("Une erreur est survenue lors de la sauvegarde du paramétrage!");
            }
        }

        public async Task<ApiResponse<string>> UpdateAsync(Guid id, SettingDto dto)
        {
            var setting = await _context.Settings.FirstOrDefaultAsync(s => s.Uid == id);

            if (setting == null)
                return ApiResponse<string>.Fail("Paramétrage introuvable");

            var settingKeyCheck = _context.Settings.Any(s => s.Key == dto.Key && s.Uid != id);
            if (settingKeyCheck)
                return ApiResponse<string>.Fail("La clé est déjà utilisée!");

            try
            {
                setting.Key = dto.Key;
                setting.Value = dto.Value;
                setting.UpdatedAt = DateTime.UtcNow;

                _context.SaveChanges();

                return ApiResponse<string>.SuccessResponse(setting.Id.ToString(), "Paramétrage mis à jour");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail("Erreur mise à jour");
            }
        }
    }
}
