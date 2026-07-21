using API.Domain.Entities.Enum;

namespace API.Application.DTO
{
    public class ProfilDto
    {
        public Guid? Id { get; set; }
         
        public string? UserName { get; set; }

        public string? Userag { get; set; } 

        public string? Email { get; set; }

        public EnumProfil TypeProfile { get; set; } 

        public string? Status { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}