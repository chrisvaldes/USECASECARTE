namespace API.Domain.Entities
{
    public class CompteDebiteRedevCarte
    {
        public Guid Id { get; set; }
        public string Ncp { get; set; } = string.Empty;
        public long Mon { get; set; }
        public string Dco { get; set; } = string.Empty;
        public string? Lib { get; set; }
    }
}