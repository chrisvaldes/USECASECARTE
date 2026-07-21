 

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Use_Case_Carte.Components.Route;
using Use_Case_Carte.Models;
using Use_Case_Carte.Services;

namespace Use_Case_Carte.Components.Pages.Roles;

public partial class CreateRole : ComponentBase, IDisposable
{
    private readonly ILogger<CreateRole> _logger;

    public CreateRole(ILogger<CreateRole> logger)
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

    protected CreateRoleDto roleModel = new();

    protected List<PermissionTreeDto> permissionsTree = new();

    protected bool isSaving;

    private DotNetObjectReference<CreateRole>? _dotNetRef;

    // protected override async Task OnInitializedAsync()
    // {
    //     permissionsTree = await PermissionService.GetAllAsync();

    //     _logger.LogInformation("{Count} permissions récupérées", permissionsTree.Count);
    // }
    protected override async Task OnInitializedAsync()
{
    permissionsTree = await PermissionService.GetAllAsync();

    _logger.LogInformation("{Count} permissions récupérées", permissionsTree.Count);

    await InvokeAsync(StateHasChanged);
}

    // protected override async Task OnAfterRenderAsync(bool firstRender)
    // {
    //     if (firstRender && permissionsTree.Any())
    //     {
    //         _dotNetRef = DotNetObjectReference.Create(this);

    //         await JS.InvokeVoidAsync(
    //             "initPermissionTree",
    //             "permissionTreeContainer",
    //             permissionsTree,
    //             _dotNetRef);
    //     }
    // }
    private bool _treeInitialized;
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_treeInitialized && permissionsTree.Any())
        {
            _treeInitialized = true;

            _dotNetRef = DotNetObjectReference.Create(this);

            await JS.InvokeVoidAsync(
                "initPermissionTree",
                "permissionTreeContainer",
                permissionsTree,
                _dotNetRef);
        }
    }

    protected async Task OnValiderPermissions()
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

            var response = await RoleService.CreateAsync(roleModel);

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
            _logger.LogError(ex, "Erreur lors de la création du rôle");
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