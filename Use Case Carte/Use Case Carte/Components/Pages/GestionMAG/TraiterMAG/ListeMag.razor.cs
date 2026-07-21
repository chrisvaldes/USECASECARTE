using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Use_Case_Carte.Components.Route;
using Use_Case_Carte.Models;
using Use_Case_Carte.Services;

namespace Use_Case_Carte.Components.Pages.GestionMAG.TraiterMAG;

public partial class ListeMag
{
    [Inject]
    public NavigationService NavigationService { get; set; } = default!;

    protected IEnumerable<TypeMag> typeMags = new List<TypeMag>();

    [Inject]
    public IJSRuntime JS { get; set; } = default!;

    [Inject]
    public TypeMagService TypeMagService { get; set; } = default!;

    // utiliser un contructeur pour écouter à chaque fois la liste des mags traiter

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadListMags();
        }
    }

    private async Task LoadListMags()
    {
        await JS.InvokeVoidAsync("toggleOnLoaderAndToast");

        typeMags = await TypeMagService.GetAllMags();

        await JS.InvokeVoidAsync("toggleOffLoaderAndToast");

        StateHasChanged();
    }

    private void NouveauMag()
    {
        NavigationService.GoNouveauMAG();
    }

    public void GoToSyntheseMag(TypeMag typeMag)
    {
        Console.WriteLine("go to synthese");
        NavigationService.GoSyntheseMag(typeMag);
    }

    public async Task DownloadBkmvti(TypeMag typeMag)
    {
        await TypeMagService.GetBkmvti(typeMag);
    }
    public async Task DownloadCarteAReguler(TypeMag typeMag)
    {
        await TypeMagService.GetCarteAReguler(typeMag);
    }

    private async Task OnCancel()
    {
        NavigationService.GoGestionMAG();
        await Task.CompletedTask;
    }
}
