using Domain.Common;
using Shared.Attributes;

namespace API.Domain.Entities
{
    [PermissionModule(nameof(TypeMag))]
    public class TypeMag : BaseEntity
    {
        public Guid Id { get; set; }
        public string? TypeMags { get; set; }
        public string? Description { get; set; }
        public string? Email { get; set; }
        public bool IsAlreadyDownload { get; set; } = false;
        public DateTimeOffset PeriodeDebut { get; set; }
        public DateTimeOffset PeriodeFin { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
