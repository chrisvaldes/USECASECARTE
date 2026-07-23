using Microsoft.AspNetCore.Components;

namespace Use_Case_Carte.Components.Shared
{
    public abstract class PermissionComponentBase : ComponentBase
    {
        [Inject]
        protected PermissionServiceAuth PermissionServiceAuth { get; set; } = default!;

        protected HashSet<string> Permissions { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                Permissions = await PermissionServiceAuth.GetPermissions();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur chargement permissions: {ex.Message}");
            }

            await base.OnInitializedAsync();
        }

        protected bool HasPermission(string permission) => Permissions.Contains(permission);
    }
}
