using Auth.Application.DTO;
using Auth.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Application.Services
{
    public class LdapService : ILdapService
    {
        public async Task<bool> AuthenticateAsync(string username, string password, LdapConfig config)
        {
            var ldapPath = config.LDAPDirectory + config.LDAPDomain;
            var adminLogin = config.ElementConnectLDAP == "2"
                ? config.LDAPMatricule
                : config.LDAPEmail;

            // First validate admin connection
            using (var userEntry = new System.DirectoryServices.DirectoryEntry(ldapPath, username, password))
            {
                try
                {
                    // Verify admin connection
                    _ = userEntry.NativeObject;

                    // Search for user
                    using (var searcher = new DirectorySearcher(userEntry))
                    {
                        searcher.Filter = $"(sAMAccountName={username})";
                        var result = searcher.FindOne();

                        if (result == null) return false;

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
    }
}
