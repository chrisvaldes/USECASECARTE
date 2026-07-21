using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Application.DTO
{
    public class  CreateUserDto
    {
        public string Matricule { get; set; } = null!;
        public string Nom { get; set; } = null!;
        public string? Prenom { get; set; }
        public string Email { get; set; } = null!;
        public string Type { get; set; } = "LDAP";
        public string Password { get; set; } = "Password@123";
        public List<string> RoleIds { get; set; } = new();
    }
}
