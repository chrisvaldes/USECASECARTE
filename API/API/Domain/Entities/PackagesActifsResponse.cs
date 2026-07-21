namespace API.Domain.Entities
{
    public class PackagesActifsResponse
    {
        public string Ncpf { get; set; } = string.Empty;
        public string Cpack { get; set; } = string.Empty;
        public string Lib { get; set; } = string.Empty;
        public DateTime Ddsou { get; set; }
    }
}