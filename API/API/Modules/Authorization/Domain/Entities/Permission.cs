using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Entities
{
    public class Permission
    {
        [Key]
        public long Id { get; set; }

        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;

        public long? ParentId { get; set; }

        [ForeignKey(nameof(ParentId))]
        public Permission? Parent { get; set; }

        public List<Permission> Children { get; set; } = new();
    }
}
