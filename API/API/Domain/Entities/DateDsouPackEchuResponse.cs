namespace API.Domain.Entities
{
    public class DateDsouPackEchuResponse
    {
        public string Ncpf { get; set; } = string.Empty;
        public string? Cpack { get; set; }
        public DateTime Ddsou { get; set; }
        public DateTime Dfsou { get; set; }
    }
}