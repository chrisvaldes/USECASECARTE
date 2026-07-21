using System.ComponentModel.DataAnnotations;

namespace Use_Case_Carte.Models.Auth
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Le nom utilisateur est obligatoire")]
        public string Username { get; set; } = string.Empty;
        [Required(ErrorMessage = "Le mot de passe est obligatoire")]
        public string Password { get; set; } = string.Empty;
    }
}
