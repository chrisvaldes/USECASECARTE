using Auth.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Application.Interfaces
{
    public interface ILdapService
    {
        public Task<bool> AuthenticateAsync(string username, string password, LdapConfig config);
    }
}
