using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Use_Case_Carte.Components.Skeletor;

public partial class Aside : ComponentBase
{
    [Inject]
    protected PermissionServiceAuth PermissionServiceAuth { get; set; } = default!;

    protected HashSet<string> Permissions = new();

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            Permissions = await PermissionServiceAuth.GetPermissions();

            Console.WriteLine("===== PERMISSIONS CHARGEES DANS ASIDE =====");
            foreach (var p in Permissions)
            {
                Console.WriteLine($" - {p}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur chargement permissions: {ex.Message}");
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Attendre que le DOM soit complètement mis à jour par Blazor
            await Task.Delay(100);
            await JS.InvokeVoidAsync("reinitUi");
        }
    }

    protected bool HasPermission(string permission)
        => Permissions.Contains(permission);
}
