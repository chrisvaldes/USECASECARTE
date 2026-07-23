using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Use_Case_Carte.Components.Route;
using Use_Case_Carte.Models;
using Use_Case_Carte.Services;

namespace Use_Case_Carte.Components.Pages.Utilisateur
{
    public partial class ListeUtilisateur : ComponentBase
    {
        [Inject]
        protected UserService UserService { get; set; } = default!;

        [Inject]
        private NavigationService NavigationService { get; set; } = default!;

        [Inject]
        private ToastService ToastService { get; set; } = default!;

        [Inject]
        private IJSRuntime JS { get; set; } = default!;

        public IEnumerable<UserDto> UserDtos = new List<UserDto>();

        public string searchQuery = "";

        // Modal de confirmation de suppression
        private bool showDeleteModal = false;
        private Guid userToDeleteId;
        private string userToDeleteName = "";
        private bool isDeleting = false;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JS.InvokeVoidAsync("toggleOnLoaderAndToast");

                await LoadProfils();

                await JS.InvokeVoidAsync("toggleOffLoaderAndToast");
            }
        }

        private async Task LoadProfils()
        {
            UserDtos = await UserService.GetAllUsers();

            StateHasChanged();
        }

        public UserDto SelectedProfil = new();

        private void GoModifierUtilisateur(UserDto userDto)
        {
            NavigationService.GoModifierUtilisateur(userDto);
        }

        private async Task OnCancel()
        {
            NavigationService.GoProfil();
            await Task.CompletedTask;
        }

        public async Task NouvelleUtilisateur()
        {
            NavigationService.GoNouveauUtilisateur();
        }

        private void OnShowDeleteModal(UserDto userDto)
        {
            userToDeleteId = userDto.Id;
            userToDeleteName = $"{userDto.Nom} {userDto.Prenom}".Trim();
            showDeleteModal = true;
        }

        private void OnCloseDeleteModal()
        {
            showDeleteModal = false;
            userToDeleteId = Guid.Empty;
            userToDeleteName = "";
        }

        private async Task OnConfirmDelete()
        {
            if (isDeleting)
                return;

            isDeleting = true;

            try
            {
                var resp = await UserService.DeleteUser(userToDeleteId);

                if (resp is { Success: true })
                {
                    ToastService.ShowSuccess("Utilisateur supprimé avec succès");
                    showDeleteModal = false;
                    await LoadProfils();
                }
                else
                {
                    ToastService.ShowError(resp?.Message ?? "Erreur lors de la suppression");
                }
            }
            finally
            {
                isDeleting = false;
                showDeleteModal = false;
                StateHasChanged();
            }
        }

    }
}
