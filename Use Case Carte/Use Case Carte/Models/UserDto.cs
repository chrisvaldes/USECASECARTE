using System.ComponentModel.DataAnnotations;

namespace Use_Case_Carte.Models
{
    public class UserDto
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Le nom est obligatoire")]
        public string? Nom { get; set; }

        [Required(ErrorMessage = "Le prénom est obligatoire")]
        public string? Prenom { get; set; }

        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "Email invalide")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Le matricule est obligatoire")]
        public string? Matricule { get; set; }
        public string FullName { get; set; } = null!;
        public string Type { get; set; } = "DEFAULT";
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new();
        public List<string> RoleIds { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
