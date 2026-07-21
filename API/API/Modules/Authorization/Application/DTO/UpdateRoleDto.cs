using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.DTO
{
    public class UpdateRoleDto
    {
        public string Name { get; set; } = default!;
        public List<long> Permissions { get; set; } = new();
    }
}
