using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Auth.Application.Interfaces;
using Auth.Application.Services;
using Authorization.Application.Interfaces;
using Authorization.Domain.Entities;
using Infrastructure.Application.Interfaces;
using Infrastructure.Application.Services;
using Infrastructure.Persistence;
//using Infrastructure.Persistence.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Organigramme.Application.Interfaces;
using Settings.Application.Interfaces;
using Users.Application.Interfaces;
using Users.Domain.Entities;

namespace Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services
                .AddIdentityCore<ApplicationUser>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                })
                .AddRoles<Role>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ILoginTrackingService, LoginTrackingService>();
            services.AddScoped<ITypeOrganisationService, TypeOrganisationService>();
            services.AddScoped<IOrganisationService, OrganisationService>();
            services.AddScoped<ISettingService, SettingService>();
            services.AddScoped<IPermissionGenerator, PermissionGenerator>();
            return services;
        }
    }
}
