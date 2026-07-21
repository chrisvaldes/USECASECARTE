namespace Use_Case_Carte.Models
{
    public class CustomerBilling
    {
        public List<BkmvtiResult> BkmvtiResults { get; set; } = new();

        public List<BkmvtiSyntheseDto> CustomerBillingSynthese { get; set; } = new();

    }
}