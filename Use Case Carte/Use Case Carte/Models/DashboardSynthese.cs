namespace Use_Case_Carte.Models
{
     public class DashboardSynthese
 {
     public List<DashboardResult> DashboardResult { get; set; } = new();
     public List<BkmvtiSyntheseDto> BkmvtiSyntheseDto { get; set; } = new();
 }
}