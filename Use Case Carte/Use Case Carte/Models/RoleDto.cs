

namespace Use_Case_Carte.Models
{
    public class RoleDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public List<string> Permissions { get; set; } = new();
    }
}
