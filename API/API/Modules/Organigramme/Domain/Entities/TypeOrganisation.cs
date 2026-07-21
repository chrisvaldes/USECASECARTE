using Shared.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Organigramme.Domain.Entities
{
    [PermissionModule("TYPE ORGANISATION")]
    public class TypeOrganisation
    {
        [Key]
        public long Id { get; set; }
        public Guid Uid { get; set; } = Guid.NewGuid();
        public string Nom { get; set; }
        public long? ParentId { get; set; }
        [ForeignKey("ParentId")]
        public TypeOrganisation Parent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
