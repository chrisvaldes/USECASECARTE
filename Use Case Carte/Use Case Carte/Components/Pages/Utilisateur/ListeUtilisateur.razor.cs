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
        public IEnumerable<UserDto> UserDtos = new List<UserDto>();

        [Inject]
        private IJSRuntime JS { get; set; } = default!;

        public string searchQuery = "";

        // protected override async Task OnInitializedAsync(){

        // }

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

    }
}