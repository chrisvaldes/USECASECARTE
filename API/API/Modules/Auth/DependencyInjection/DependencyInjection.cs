using Auth.Application.Interfaces;
using Auth.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAuthModule(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ILdapService, LdapService>();
            return services;
        }
    }
}
