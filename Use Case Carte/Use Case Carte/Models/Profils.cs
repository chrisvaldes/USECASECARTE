using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Use_Case_Carte.Models.Enum;

namespace Use_Case_Carte.Models
{
    public class ProfilModel
    {
        public Guid Id { get; set; } 

        public string? UserName { get; set; } = string.Empty;

        public string? Userag { get; set; } = string.Empty;

        public string? Email { get; set; } = string.Empty;

        public EnumProfil TypeProfile { get; set; } = EnumProfil.ADMIN;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EnumStatut Status { get; set; } = EnumStatut.Actif;

        public bool IsDeleted { get; set; } = false;
    }
}
