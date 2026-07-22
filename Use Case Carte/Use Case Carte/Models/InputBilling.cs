namespace Use_Case_Carte.Models
{
    public class InputBilling
    {
        public DateTime Debut { get; set; } = DateTime.Now;
        public DateTime Fin { get; set; } = DateTime.Now;
        public string? NumeroCompte { get; set; }
    }
}
