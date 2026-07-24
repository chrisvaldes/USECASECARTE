using Microsoft.AspNetCore.Components;

namespace Use_Case_Carte.Components.Pages;

public partial class AccessDenied : ComponentBase
{
    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    protected void GoToHome()
    {
        Navigation.NavigateTo("/");
    }
}

