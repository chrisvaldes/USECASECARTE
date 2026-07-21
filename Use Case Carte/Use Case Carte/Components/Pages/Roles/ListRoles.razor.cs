using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Use_Case_Carte.Components.Route;
using Use_Case_Carte.Models;
using Use_Case_Carte.Services;

namespace Use_Case_Carte.Components.Pages.Roles
{
    public partial class ListRoles : ComponentBase
    {
        private readonly ILogger<ListRoles> _logger;

        public ListRoles(ILogger<ListRoles> logger)
        {
            _logger = logger;
        }

        [Inject]
        private RoleService RoleService { get; set; } = default!;

        [Inject]
        private NavigationService NavigationService { get; set; } = default!;

        [Inject]
        private ToastService ToastService { get; set; } = default!;

        [Inject]
        private IJSRuntime JS { get; set; } = default!;

        protected List<RoleDto> roles = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadRoles();
        }

        private async Task LoadRoles()
        {
            try
            {
                var result = await RoleService.GetAllRoles();

                roles = result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des rôles");
            }
        }

        protected void NouveauRole()
        {
            NavigationService.GoCreerRole();
        }

        protected void ModifierRole(string roleId)
        {
            NavigationService.GoModifierRole(roleId);
        }

        protected async Task SupprimerRole(string roleId)
        {
            try
            {
                if (!Guid.TryParse(roleId, out var guidId))
                {
                    ToastService.ShowError("Identifiant invalide");
                    return;
                }

                await JS.InvokeVoidAsync("toggleOnLoaderAndToast");

                var response = await RoleService.DeleteAsync(guidId);

                if (response.Success)
                {
                    ToastService.ShowSuccess("Rôle supprimé avec succès");
                    await LoadRoles();
                }
                else
                {
                    ToastService.ShowError(response.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du rôle");
                ToastService.ShowError("Erreur lors de la suppression du rôle");
            }
            finally
            {
                await JS.InvokeVoidAsync("toggleOffLoaderAndToast");
                StateHasChanged();
            }
        }

        protected async Task OnCancel()
        {
            NavigationService.GoRole();
            await Task.CompletedTask;
        }
    }
}

