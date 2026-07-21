using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Application.DTO
{
    public class LdapConfig
    {
        public string? LDAPDirectory { get; set; }
        public string? LDAPDomain { get; set; }
        public string? LDAPEmail { get; set; }
        public string? LDAPMatricule { get; set; }
        public string? LDAPPassword { get; set; }
        public string? ElementConnectLDAP { get; set; }
    }
}
