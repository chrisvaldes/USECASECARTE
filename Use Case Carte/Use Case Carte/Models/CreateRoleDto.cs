namespace Use_Case_Carte.Models
{
    public class CreateRoleDto
    {
        public string Name { get; set; } = default!;
        public List<long> Permissions { get; set; } = new();
    }
}