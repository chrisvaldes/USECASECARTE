using System.Text.Json; // ajouté
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Use_Case_Carte.Components.Route;
using Use_Case_Carte.Models;
using Use_Case_Carte.Services;

namespace Use_Case_Carte.Components.Pages.Profils;

public partial class CreateProfil : ProtectedPageBase
{
    protected override string[] RequiredPermissions => new[] { "UTILISATEUR" };

    [Inject]
    private NavigationService NavigationService { get; set; } = default!;

    [Inject]
    protected ProfilService ProfilService { get; set; } = default!;

    [Parameter]
    public int? Id { get; set; }

    private ProfilModel profilModel = new();

    private bool IsUpdate => Id.HasValue;

    [Parameter]
    public EventCallback OnSaved { get; set; }

    [Inject]
    public ToastService ToastService { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        if (Id.HasValue)
        {
            // profilModel = await ProfilService.GetProfilById(Id.Value);
        }
        else
        {
            profilModel = new ProfilModel();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // await JS.InvokeVoidAsync("initProfileModals");
        }
    }

    // Handler conseillé : reçoit EditContext, retourne Task
    // private async Task OnSaveProfil()
    // {
    //     var resp = await ProfilService.Save(profilModel);

    //     if (resp?.Success == true)
    //     {
    //         profilModel = new ProfilModel();
    //         NavigationService.GoProfil();
    //         ToastService.ShowSuccess(resp.Message);
    //         await Task.Delay(3000);

    //         await OnSaved.InvokeAsync();
    //     }
    //     else
    //     {
    //         ToastService.ShowError(resp?.Message ?? "Erreur lors de l'enregistrement");
    //     }
    // }
    private async Task OnSaveProfil()
    {
        var resp = await ProfilService.Save(profilModel);

        if (resp?.Success == true)
        {
            profilModel = new ProfilModel(); 
            
            // 1. Afficher le toast AVANT toute navigation
            ToastService.ShowSuccess(resp.Message);

            // 2. Laisser le temps au toast de s'afficher/être visible
            await Task.Delay(3000);

            // 3. Naviguer seulement après
            NavigationService.GoProfil();

            await OnSaved.InvokeAsync();
        }
        else
        { 
            await JS.InvokeVoidAsync("showToast", "message", "warning");
            ToastService.ShowError(resp!.Message);
            // 2. Laisser le temps au toast de s'afficher/être visible
            await Task.Delay(2000);
        }
    }

    private async Task OnCancel()
    {
        NavigationService.GoProfil();
        await Task.CompletedTask;
    }
}
