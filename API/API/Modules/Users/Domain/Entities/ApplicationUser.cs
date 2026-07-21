using Microsoft.AspNetCore.Identity;
using Shared.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Domain.Entities
{
    [PermissionModule("UTILISATEUR")]
    [CustomPermission("ACTIVER_UTILISATEUR", "ACTIVER UTILISATEUR")]
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string Matricule { get; set; }
        public string Nom { get; set; }
        public string? Prenom { get; set; }
        public string FullName => $"{Nom} {Prenom}";
        public string Type { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        [NotMapped]
        public List<string>? RoleIds { get; set; }
    }
}
