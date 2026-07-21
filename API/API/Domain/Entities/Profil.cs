using System.ComponentModel.DataAnnotations;
using API.Domain.Entities.Enum;
using Domain.Common;
using Shared.Attributes;

namespace API.Domain.Entities
{
    [PermissionModule(nameof(Profil))]
    public class Profil : BaseEntity
    {
        [Required(ErrorMessage = "Le nom d'utilisateur est obligatoire")]
        public string? UserName { get; set; }

        public string? Userag { get; set; }

        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Le type de profil est obligatoire")]
        [EnumDataType(typeof(EnumProfil), ErrorMessage = "Type de profil invalide")]
        public EnumProfil TypeProfile { get; set; }

        [Required(ErrorMessage = "Le statut est obligatoire")]
        [RegularExpression("Actif|Inactif", ErrorMessage = "Statut invalide")]
        public string? Status { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
