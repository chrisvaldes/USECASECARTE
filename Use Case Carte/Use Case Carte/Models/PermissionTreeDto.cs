namespace Use_Case_Carte.Models
{
    public class PermissionTreeDto
    {
        public string Id { get; set; } = default!;
        public string Parent { get; set; } = default!;
        public string Text { get; set; } = default!;
        public string Code { get; set; } = default!;
    }
}