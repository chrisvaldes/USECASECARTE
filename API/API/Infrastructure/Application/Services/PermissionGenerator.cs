using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Application.Services
{
    using Authorization.Domain.Entities;
    using Infrastructure.Application.Interfaces;
    using Infrastructure.Persistence;
    using Microsoft.EntityFrameworkCore;
    using Shared.Attributes;
    using System.Reflection;

    public class PermissionGenerator : IPermissionGenerator
    {
        private readonly ApplicationDbContext _context;

        public PermissionGenerator(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task GenerateAsync()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var entities = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetCustomAttribute<PermissionModuleAttribute>() != null)
                .ToList();

            foreach (var entity in entities)
            {
                var moduleAttr = entity.GetCustomAttribute<PermissionModuleAttribute>()!;
                var moduleCode = moduleAttr.Name.ToUpper();

                // 1. MODULE (parent)
                var parent = await GetOrCreatePermission(
                    code: moduleCode,
                    name: moduleCode,
                    parent: null
                );

                // 2. CRUD
                await GenerateCrudPermissions(parent, moduleCode);

                // 3. CUSTOM
                await GenerateCustomPermissions(entity, parent);
            }

            await _context.SaveChangesAsync();
        }

        // =========================
        // CRUD
        // =========================
        private async Task GenerateCrudPermissions(Permission parent, string moduleCode)
        {
            var actions = new[] { "CREER", "CONSULTER", "MODIFIER", "SUPPRIMER" };

            foreach (var action in actions)
            {
                var code = $"{moduleCode}_{action}";
                var name = $"{action} {moduleCode}";

                await GetOrCreatePermission(code, name, parent);
            }
        }

        // =========================
        // CUSTOM
        // =========================
        private async Task GenerateCustomPermissions(Type entity, Permission parent)
        {
            var customPermissions = entity.GetCustomAttributes<CustomPermissionAttribute>();

            foreach (var custom in customPermissions)
            {
                await GetOrCreatePermission(
                    code: custom.Code,
                    name: custom.Name,
                    parent: parent
                );
            }
        }

        // =========================
        // UPSERT
        // =========================
        private async Task<Permission> GetOrCreatePermission(
    string code,
    string name,
    Permission? parent)
        {
            var existing = await _context.Set<Permission>()
                .FirstOrDefaultAsync(p => p.Code == code);

            if (existing != null)
                return existing;

            var permission = new Permission
            {
                Code = code,
                Name = name,
                Parent = parent
            };

            _context.Set<Permission>().Add(permission);

            return permission;
        }
    }
}
