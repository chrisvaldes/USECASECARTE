
using Microsoft.AspNetCore.Identity;
using Shared.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Entities
{
    [PermissionModule("ROLE")]
    public class Role : IdentityRole<Guid>
    {
        public List<RolePermission> RolePermissions { get; set; } = new();
    }
}
