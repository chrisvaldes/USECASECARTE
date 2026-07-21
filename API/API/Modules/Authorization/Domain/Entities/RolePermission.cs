using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Entities
{
    public class RolePermission
    {
        
        public Guid Id { get; set; }
        public Guid RoleId { get; set; }
        public Role Role { get; set; } = default!;

        public long PermissionId { get; set; }
        public Permission Permission { get; set; } = default!;
    }
}
