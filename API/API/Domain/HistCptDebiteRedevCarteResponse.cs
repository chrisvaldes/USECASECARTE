namespace API.Domain.Entities
{
    public class HistCptDebiteRedevCarteResponse
    {
        public string Ncp { get; set; } = string.Empty;
        public long Mon { get; set; }
        public DateTime Dco { get; set; }
        public string? Lib { get; set; }
    }
}