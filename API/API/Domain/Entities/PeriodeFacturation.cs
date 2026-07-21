namespace API.Domain.Entities
{
    public class PeriodeFacturation
    {
        public DateTimeOffset Debut { get; set; }

        public DateTimeOffset Fin { get; set; }

        public int NombreMois { get; set; }
    }
}