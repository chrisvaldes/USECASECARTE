using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Use_Case_Carte.Models
{
    public class UpdateUserDto
    {
        public string Matricule { get; set; } = null!;
        public string Nom { get; set; } = null!;
        public string? Prenom { get; set; }
        public string Email { get; set; } = null!;
        public string Type { get; set; } = null!;
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}