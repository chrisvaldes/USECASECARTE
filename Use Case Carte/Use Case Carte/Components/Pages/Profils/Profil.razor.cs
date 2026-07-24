using Microsoft.AspNetCore.Components;
using Use_Case_Carte.Components.Route;

namespace Use_Case_Carte.Components.Pages.Profils;

public partial class Profil : ProtectedPageBase
{
    protected override string[] RequiredPermissions => new[] { "UTILISATEUR" };

    [Inject]
    private NavigationService NavigationService { get; set; } = default!;

    public async Task CreateProfil()
    {
        NavigationService.GoCreerProfil();
    }
}
