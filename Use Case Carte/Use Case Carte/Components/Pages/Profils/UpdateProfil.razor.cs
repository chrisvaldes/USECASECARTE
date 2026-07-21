using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Use_Case_Carte.Components.Route;
using Use_Case_Carte.Models;
using Use_Case_Carte.Services;

namespace Use_Case_Carte.Components.Pages.Profils;

public partial class UpdateProfil
{
    [Inject]
    public ProfilService ProfilService { get; set; } = default!;

    [Inject]
    public ToastService ToastService { get; set; } = default!;

    [Inject]
    public NavigationService NavigationService { get; set; } = default!;

    [Inject]
    public IJSRuntime JS { get; set; } = default!;

    [Parameter]
    public Guid Id { get; set; }

    private ProfilModel profilModel = new();

    private bool loaded = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !loaded)
        {
            loaded = true;

            Console.WriteLine($"Id reçu : {Id}");

            var profil = await ProfilService.GetById(Id);

            if (profil != null)
            {
                profilModel = profil;
                StateHasChanged();
            }
        }
    }

    private async Task OnUpdateProfil(Guid id, ProfilModel profilModel)
    {
        var resp = await ProfilService.UpdateProfil(id, profilModel);

        if (resp!.Success)
        {
            ToastService.ShowSuccess(resp.Message);
            await Task.Delay(3000);
            NavigationService.GoProfil();
        }
        else
        {
            ToastService.ShowError(resp.Message);
            await Task.Delay(3000);
            await JS.InvokeVoidAsync("toggleOffLoaderAndToast");
        }
    }

    private async Task OnCancel()
    {
        NavigationService.GoProfil();
        await Task.CompletedTask;
    }
}
