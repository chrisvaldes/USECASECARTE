using Microsoft.AspNetCore.Components;

namespace Use_Case_Carte.Components.Pages.Roles
{
    public partial class ListRoles : ComponentBase
    {
        private readonly ILogger<ListRoles> _logger;

        public ListRoles(ILogger<ListRoles> logger)
        {
            _logger = logger;
        }

        public void OnGet() { }
    }
}
