using Microsoft.AspNetCore.Components;
using Use_Case_Carte.Components.Route;
using Use_Case_Carte.Services;

public class ProtectedPageBase : ComponentBase
{
    [Inject]
    protected AuthService AuthService { get; set; } = default!;

    [Inject]
    protected NavigationService NavigationService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        var token = await AuthService.GetToken();

        if (string.IsNullOrWhiteSpace(token))
        {
            NavigationService.GoLogin();
        }
    }
}