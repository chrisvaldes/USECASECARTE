using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Use_Case_Carte.Models
{
    public class TypeMagWithSyntheseDto
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public string? Email { get; set; }
        public DateTimeOffset PeriodeDebut { get; set; }
        public DateTimeOffset PeriodeFin { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // Liste de synthèses par code carte
        public List<BkmvtiSyntheseDto> SyntheseBkmvtis { get; set; } =
            new List<BkmvtiSyntheseDto>();
    }
}
