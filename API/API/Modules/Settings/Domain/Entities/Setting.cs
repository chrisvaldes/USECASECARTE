using Shared.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settings.Domain.Entities
{
    [PermissionModule("CONFIGURATION")]
    public class Setting
    {
        [Key]
        public long Id { get; set; }
        public Guid Uid { get; set; } = Guid.NewGuid();

        public string Key { get; set; } = default!;

        public string Value { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
