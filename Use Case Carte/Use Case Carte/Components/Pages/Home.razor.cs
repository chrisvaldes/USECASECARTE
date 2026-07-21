 
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Use_Case_Carte.Services;
using Use_Case_Carte.Models.Auth;
using Use_Case_Carte.Components.Route;

namespace Use_Case_Carte.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject]
    public ToastService ToastService { get; set; } = default!;

    [Inject]
    private AuthService AuthService { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private NavigationService NavigationService { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    protected LoginRequest loginRequest = new();

 

    protected async Task LoginAsync()
    {
        try
        {
            var result = await AuthService.Login(loginRequest);

            if (result.Success)
            {
                ToastService.ShowSuccess(result.Message);

                await Task.Delay(3000);

                NavigationService.GoProfil();
            }
            else
            {
                ToastService.ShowError(result.Message ?? "Erreur");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

}