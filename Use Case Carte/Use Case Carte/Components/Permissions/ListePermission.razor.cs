
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Use_Case_Carte.Components.Route;
using Use_Case_Carte.Models;
using Use_Case_Carte.Services;

namespace Use_Case_Carte.Components.Permissions
{
    public partial class ListePermission : ComponentBase
    {
        private readonly ILogger<ListePermission> _logger;

        public ListePermission(ILogger<ListePermission> logger)
        {
            _logger = logger;
        }

        [Inject]
        protected ProfilService ProfilService { get; set; } = default!;

        [Inject]
        private NavigationService NavigationService { get; set; } = default!;
        public IEnumerable<ProfilModel> profils = new List<ProfilModel>();

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
                // On construit l'arbre après le premier render, une fois les données chargées
                await RenderPermissionTreeAsync();

                await JS.InvokeVoidAsync("toggleOffLoaderAndToast");
            }
        }


        private async Task LoadProfils()
        {
            profils = await ProfilService.GetAllProfils();

            StateHasChanged();
        }

        public ProfilModel SelectedProfil = new();

        private void ModifierProfil(ProfilModel profil)
        {
            NavigationService.GoModifierProfil(profil);
        }

        public async Task NouvellePermission()
        {
            NavigationService.GoNouvellePermission();
        }




        [Inject]
        protected PermissionService PermissionService { get; set; } = default!;

        [Inject]
        public ToastService ToastService { get; set; } = default!;
        protected List<PermissionTreeDto> permissionsTree = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadProfilsAsync();
            await LoadPermissionsAsync();
        }



        private async Task LoadProfilsAsync()
        {
            // profils = await ProfilService.GetAllAsync();
            profils ??= new List<ProfilModel>();
        }

        private async Task LoadPermissionsAsync()
        {
            permissionsTree = await PermissionService.GetAllAsync();

            if (!permissionsTree.Any())
            {
                Console.WriteLine("---------->> Aucune permission récupérée");
            }
        }

        private DotNetObjectReference<ListePermission>? _dotNetRef;

        private async Task RenderPermissionTreeAsync()
        {
            if (permissionsTree.Any())
            {
                _dotNetRef = DotNetObjectReference.Create(this);
                await JS.InvokeVoidAsync("initPermissionTree", "permissionTreeContainer", permissionsTree, _dotNetRef);
            }
        }

        [JSInvokable]
        public void OnPermissionsChecked(string[] selectedIds)
        {
            Console.WriteLine($"Permissions cochées : {string.Join(", ", selectedIds)}");
            // Traite les IDs cochés ici, ex: stocker dans une liste, appeler un service, etc.
        }

        public void Dispose()
        {
            _dotNetRef?.Dispose();
        }

    }
}
