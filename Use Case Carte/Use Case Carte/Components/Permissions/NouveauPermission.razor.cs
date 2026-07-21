using Microsoft.AspNetCore.Components;
using Use_Case_Carte.Components.Route;
using Use_Case_Carte.Models;
using Use_Case_Carte.Services;

namespace Use_Case_Carte.Components.Permissions
{
    public partial class NouveauPermission : ComponentBase
    {
        [Inject]
        NavigationService NavigationService { get; set; } = default!;

        [Inject]
        ToastService ToastService { get; set; } = default!;

        [Inject]
        PermissionService PermissionService { get; set; } = default!;
        private readonly ILogger<NouveauPermission> _logger;
 
        public NouveauPermission(ILogger<NouveauPermission> logger)
        {
            _logger = logger;
        }

        private Permission Permission = new();

        private async Task OnSavePermission()
        {
            var resp = await PermissionService.PostPermissionAsync(Permission);

            if (resp?.Success == true)
            {
                Permission = new Permission();
                NavigationService.GoPermission();
                ToastService.ShowSuccess(resp.Message);
                await Task.Delay(3000);
            }
            else
            {
                ToastService.ShowError(resp?.Message ?? "Erreur lors de l'enregistrement");
            }
        }

        private async Task OnCancel()
        {
            NavigationService.GoPermission();
        }
    }
}
