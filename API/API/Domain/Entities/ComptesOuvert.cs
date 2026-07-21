namespace API.Domain.Entities
{
    public class ComptesOuvert
    {
        public Guid Id { get; set; }
        public string Ncp { get; set; } = string.Empty;
        public string Cfe { get; set; } = string.Empty;
        public string Clc { get; set; } = string.Empty;
        public string Cha { get; set; } = string.Empty;
        public string Age { get; set; } = string.Empty;
        public string Inti { get; set; } = string.Empty;

    }
}