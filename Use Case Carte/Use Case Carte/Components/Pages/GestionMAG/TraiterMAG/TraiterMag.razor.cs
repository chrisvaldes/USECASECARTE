using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Use_Case_Carte.Components.Route;
using Use_Case_Carte.Models;
using Use_Case_Carte.Services;

namespace Use_Case_Carte.Components.Pages.GestionMAG.TraiterMAG;

public partial class TraiterMag : ComponentBase
{
    [Inject]
    NavigationService NavigationService { get; set; } = default!;

    [Inject]
    NouveauMagService NouveauMagService { get; set; } = default!;

    private readonly ILogger<TraiterMag> _logger;

    protected InputModel InputModel { get; set; } = new();

    [Inject]
    public ToastService ToastService { get; set; } = default!;

    public TraiterMag(ILogger<TraiterMag> logger)
    {
        _logger = logger;
    }

    public async Task OnCancel()
    {
        NavigationService.GoGestionMAG();
        await Task.CompletedTask;
    }

    public async Task Traiter()
    {
        Console.WriteLine($"data : {InputModel.TypeMag}");
        try
        {
            var resultRequest = await NouveauMagService.NouveauMag(InputModel);

            if (resultRequest == null)
            {
                ToastService.ShowError("Aucune réponse du serveur");
                return;
            }

            if (!resultRequest.Success)
            {
                 
                ToastService.ShowError(resultRequest.Message);

                // Rester sur la page pour que l'utilisateur corrige
                return;
            }

            // Succès
            ToastService.ShowSuccess(resultRequest.Message ?? "Traitement lancé avec succès");

            await Task.Delay(3000);

            NavigationService.GoGestionMAG();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erreur inattendue : " + ex.Message);
            ToastService.ShowError("Erreur inattendue : " + ex.Message);
        }
    }

    private async Task OnFileSelected(InputFileChangeEventArgs e, string propertyName)
    {
        var file = e.File;

        switch (propertyName)
        {
            case nameof(InputModel.Apprint):
                InputModel.Apprint = file;
                break;

            case nameof(InputModel.OpenAccount):
                InputModel.OpenAccount = file;
                break;

            case nameof(InputModel.ActiveAccount):
                InputModel.ActiveAccount = file;
                break;

            case nameof(InputModel.ActivePackage):
                InputModel.ActivePackage = file;
                break;

            case nameof(InputModel.CtxAccount):
                InputModel.CtxAccount = file;
                break;

            case nameof(InputModel.DateLastSouPackEchu):
                InputModel.DateLastSouPackEchu = file;
                break;

            case nameof(InputModel.AccountHisDebiteByRedevCard):
                InputModel.AccountHisDebiteByRedevCard = file;
                break;
        }
    }
}
