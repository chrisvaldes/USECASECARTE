using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Use_Case_Carte.Components.Route;
using Use_Case_Carte.Models;
using Use_Case_Carte.Services;

namespace Use_Case_Carte.Components.Pages.Roles;

public partial class UpdateRole : ComponentBase, IDisposable
{
    private readonly ILogger<UpdateRole> _logger;

    public UpdateRole(ILogger<UpdateRole> logger)
    {
        _logger = logger;
    }

    [Inject]
    private PermissionService PermissionService { get; set; } = default!;

    [Inject]
    private RoleService RoleService { get; set; } = default!;

    [Inject]
    private NavigationService NavigationService { get; set; } = default!;

    [Inject]
    private ToastService ToastService { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Parameter]
    public Guid Id { get; set; }

    protected UpdateRoleDto roleModel = new();

    protected List<PermissionTreeDto> permissionsTree = new();

    protected bool isSaving;
    protected bool isLoading = true;

    private DotNetObjectReference<UpdateRole>? _dotNetRef;
    private bool _treeInitialized;
    private string[] _preSelectedIds = Array.Empty<string>();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            // Charger l'arbre des permissions
            permissionsTree = await PermissionService.GetAllAsync();
            _logger.LogInformation("{Count} permissions récupérées", permissionsTree.Count);

            // Charger le rôle à modifier
            var response = await RoleService.GetByIdAsync(Id);
            if (response != null && response.Success && response.Data != null)
            {
                roleModel.Name = response.Data.Name;

                // Récupérer les codes de permission du rôle et les convertir en IDs
                var permissionCodes = response.Data.Permissions ?? new List<string>();
                _preSelectedIds = permissionsTree
                    .Where(p => permissionCodes.Contains(p.Code))
                    .Select(p => p.Id)
                    .ToArray();

                _logger.LogInformation("Rôle chargé : {Name} avec {Count} permissions",
                    roleModel.Name, _preSelectedIds.Length);
            }
            else
            {
                ToastService.ShowError(response?.Message ?? "Impossible de charger le rôle");
                NavigationService.GoRole();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du chargement du rôle");
            ToastService.ShowError("Erreur lors du chargement du rôle");
            NavigationService.GoRole();
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_treeInitialized && permissionsTree.Any() && !isLoading)
        {
            _treeInitialized = true;

            _dotNetRef = DotNetObjectReference.Create(this);

            // Initialiser l'arbre avec les permissions pré-sélectionnées
            await JS.InvokeVoidAsync(
                "initPermissionTree",
                "permissionTreeContainer",
                permissionsTree,
                _dotNetRef,
                _preSelectedIds);  // Ajouter les IDs pré-sélectionnés
        }
    }

    protected async Task OnValiderModification()
    {
        isSaving = true;

        try
        {
            await JS.InvokeVoidAsync("toggleOnLoaderAndToast");

            var checkedIds = await JS.InvokeAsync<string[]>(
                "getCheckedPermissions",
                "permissionTreeContainer");

            roleModel.Permissions = checkedIds
                .Select(long.Parse)
                .ToList();

            if (!roleModel.Permissions.Any())
            {
                ToastService.ShowError("Sélectionnez au moins une permission.");
                return;
            }

            var response = await RoleService.UpdateAsync(Id, roleModel);

            if (response.Success)
            {
                ToastService.ShowSuccess(response.Message);
                NavigationService.GoRole();
            }
            else
            {
                ToastService.ShowError(response.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la modification du rôle");
            ToastService.ShowError("Une erreur est survenue.");
        }
        finally
        {
            isSaving = false;
            await JS.InvokeVoidAsync("toggleOffLoaderAndToast");
            StateHasChanged();
        }
    }

    protected void Annuler()
    {
        NavigationService.GoRole();
    }

    public void Dispose()
    {
        _dotNetRef?.Dispose();
    }
}

