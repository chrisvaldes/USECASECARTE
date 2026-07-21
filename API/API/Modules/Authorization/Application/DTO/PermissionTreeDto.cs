using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.DTO
{
    public class PermissionTreeDto
    {
        public string Id { get; set; } = default!;
        public string Parent { get; set; } = default!;
        public string Text { get; set; } = default!;
        public string Code { get; set; } = default!;
    }
}
