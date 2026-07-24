using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Use_Case_Carte.Components.Route;
using Use_Case_Carte.Models;
using Use_Case_Carte.Services;

namespace Use_Case_Carte.Components.Pages.Utilisateur
{
    public partial class UpdateUtilisateur : ProtectedPageBase
    {
        protected override string[] RequiredPermissions => new[] { "UTILISATEUR" };

        [Inject]
        public UserService UserService { get; set; } = default!;

        [Inject]
        public RoleService RoleService { get; set; } = default!;

        [Inject]
        public ToastService ToastService { get; set; } = default!;

        [Inject]
        public NavigationService NavigationService { get; set; } = default!;

        [Inject]
        public IJSRuntime JS { get; set; } = default!;

        [Parameter]
        public Guid Id { get; set; }

        private UpdateUserDto UpdateUserDto = new();
        private IEnumerable<RoleDto> AllRoles = new List<RoleDto>();
        private string[] SelectedRoles = Array.Empty<string>();

        private bool loaded = false;
        private bool isSubmitting = false;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !loaded)
            {
                loaded = true;

                var userTask = UserService.GetById(Id);
                var rolesTask = RoleService.GetAllRoles();

                await Task.WhenAll(userTask, rolesTask);

                var userResult = await userTask;
                AllRoles = await rolesTask ?? new List<RoleDto>();

                if (userResult != null)
                {
                    // Mapping UserDto (lecture) -> UpdateUserDto (formulaire d'update)
                    UpdateUserDto = new UpdateUserDto
                    {
                        Matricule = userResult.Matricule ?? "",
                        Nom = userResult.Nom ?? "",
                        Prenom = userResult.Prenom,
                        Email = userResult.Email ?? "",
                        Type = userResult.Type,
                        IsActive = userResult.IsActive,
                        Roles = userResult.Roles, // noms de rôles déjà assignés
                    };

                    SelectedRoles = UpdateUserDto.Roles.ToArray();
                }

                StateHasChanged();
            }
        }

        private async Task OnUpdateUser()
        {
            if (isSubmitting)
                return;

            isSubmitting = true;

            try
            {
                // Map SelectedRoles (role names) → role IDs
                UpdateUserDto.Roles = AllRoles
                    .Where(r => SelectedRoles.Contains(r.Name))
                    .Select(r => r.Id)
                    .ToList();

                var resp = await UserService.UpdateUser(Id, UpdateUserDto);

                if (resp is { Success: true })
                {
                    ToastService.ShowSuccess(resp.Message);
                    await Task.Delay(3000);
                    NavigationService.GoListeUtilisateur();
                }
                else
                {
                    ToastService.ShowError(resp?.Message ?? "Erreur lors de la mise à jour");
                }
            }
            finally
            {
                isSubmitting = false;
                await JS.InvokeVoidAsync("toggleOffLoaderAndToast");
            }
        }

        private async Task OnCancel()
        {
            NavigationService.GoListeUtilisateur();
            await Task.CompletedTask;
        }
    }
}