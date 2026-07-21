using Shared.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Entities;

namespace Organigramme.Domain.Entities
{
    [PermissionModule("ORGANISATION")]
    public class Organisation
    {
        [Key]
        public long Id { get; set; }
        public Guid Uid { get; set; } = Guid.NewGuid();
        public string Code { get; set; }
        public string Nom { get; set; }
        public long TypeId { get; set; }
        [ForeignKey("TypeId")]
        public TypeOrganisation TypeOrganisation { get; set; }
        public long? ParentId { get; set; }
        [ForeignKey("ParentId")]
        public Organisation Parent { get; set; }
        public Guid ResponsableId { get; set; }
        public ApplicationUser Responsable {  get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
