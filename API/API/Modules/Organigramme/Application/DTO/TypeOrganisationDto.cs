using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Organigramme.Application.DTO
{
    public class TypeOrganisationDto
    {
        public long Id { get; set; }
        public Guid Uid { get; set; }
        public string Nom { get; set; }
        public string? Parent { get; set; }
        public long? ParentId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
