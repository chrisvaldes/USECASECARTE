namespace Use_Case_Carte.Models
{
    public class TypeMag
    {
        public Guid Id { get; set; }
        public string? TypeMags { get; set; }
        public string? Description { get; set; }
        public string? Email { get; set; }
        public bool isAlreadyDownload { get; set; } = false;
        public DateTimeOffset PeriodeDebut { get; set; }
        public DateTimeOffset PeriodeFin { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
