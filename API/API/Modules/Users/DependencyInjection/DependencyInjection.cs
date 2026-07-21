using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Application.Interfaces;
using Users.Domain.Entities;

namespace Users.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddUsersModule(this IServiceCollection services)
        {
            return services;
        }
    }
}
