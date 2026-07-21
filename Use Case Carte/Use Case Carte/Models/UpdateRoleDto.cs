namespace Use_Case_Carte.Models
{
    public class UpdateRoleDto
    {
        public string Name { get; set; } = default!;
        public List<long> Permissions { get; set; } = new();
    }
}

