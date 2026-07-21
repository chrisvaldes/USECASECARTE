namespace API.Domain.Entities
{
    public class Apprint
    {
        public string? NumCarte { get; set; }

        public string? NomPropCarte { get; set; }

        public string? LongNum { get; set; }

        public string? VhCodeCarte { get; set; }

        public string? QZero { get; set; }

        public string? DateValiditeAgenceCodeDeviseNumeroCompte { get; set; }

        public string DateCreationCarte { get; set; } = string.Empty;

        public string? EstActifCodeTarifNumeroCompte { get; set; }

        public string? CodeCarte { get; set; }

        public string? NomPrenom { get; set; }

        public string? LastProp { get; set; }
    }
}