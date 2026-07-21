using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Application.DTO
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Matricule { get; set; } = null!;
        public string Nom { get; set; } = null!;
        public string? Prenom { get; set; }
        public string FullName => $"{Nom} {Prenom}";
        public string Email { get; set; } = null!;
        public string Type { get; set; } = null!;
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new();
        public List<string> RoleIds { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}
