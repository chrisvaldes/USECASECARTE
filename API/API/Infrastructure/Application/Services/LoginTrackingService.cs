using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Application.Services
{
    public class LoginTrackingService : ILoginTrackingService
    {
        private readonly ApplicationDbContext _context;

        public LoginTrackingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task TrackAsync(LoginHistory history)
        {
            _context.Set<LoginHistory>().Add(history);
            await _context.SaveChangesAsync();
        }
    }
}
