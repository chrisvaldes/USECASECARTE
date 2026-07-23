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
    protected PermissionServiceAuth PermissionServiceAuth { get; set; } = default!;

    protected HashSet<string> Permissions = new();

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    public TypeMagService TypeMagService { get; set; } = default!;

    // utiliser un contructeur pour écouter à chaque fois la liste des mags traiter

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
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadListMags();
            await Task.Delay(100);
            await JS.InvokeVoidAsync("reinitUi");
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

    protected bool HasPermission(string permission) => Permissions.Contains(permission);

    public void GoToSyntheseMag(TypeMag typeMag)
    {
        Console.WriteLine("go to synthese");
        NavigationService.GoSyntheseMag(typeMag);
    }

    public async Task DownloadBkmvti(TypeMag typeMag)
    {
        try
        {
            await TypeMagService.GetBkmvti(typeMag);

            // Marquer comme déjà téléchargé pour griser le bouton
            typeMag.isAlreadyDownload = true;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur téléchargement BKMVTI : {ex.Message}");
        }
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
