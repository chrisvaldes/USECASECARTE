namespace API.Application.DTO
{
    public class CarteARegulerDto
    {
        public DateTimeOffset DateCreationCarte { get; set; }
        public string? CodeCarte { get; set; }
        public string? Carte { get; set; }
        public string? NumeroCompte { get; set; }
        public string? CodeAgence { get; set; }
        public string? CodeTarif { get; set; }
        public string? NomClient { get; set; }
    }
}