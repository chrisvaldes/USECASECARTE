using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Entities;

namespace Organigramme.Application.DTO
{
    public class OrganisationDto
    {
        public long Id { get; set; }
        public Guid Uid { get; set; } = Guid.NewGuid();
        public string Code { get; set; }
        public string Nom { get; set; }
        public string? Parent { get; set; }
        public string? Responsable { get; set; }
        public string TypeId { get; set; }
        public string? ParentId { get; set; }
        public Guid ResponsableId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
