using System.Text.Json; // ajouté
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Use_Case_Carte.Components.Route;
using Use_Case_Carte.Models;
using Use_Case_Carte.Services;

namespace Use_Case_Carte.Components.Pages.Utilisateur;

public partial class NouveauUtilisateur : ComponentBase
{
    [Inject]
    private NavigationService NavigationService { get; set; } = default!;

    [Inject]
    protected UserService UserService { get; set; } = default!;

    [Inject]
    protected RoleService RoleService { get; set; } = default!;

    [Parameter]
    public int? Id { get; set; }

    private UserDto UserDto = new();
 
    [Inject]
    public ToastService ToastService { get; set; } = default!;

    private DotNetObjectReference<NouveauUtilisateur>? dotnetHelper { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    public IEnumerable<RoleDto> RoleDto = new List<RoleDto>();

    protected override async Task OnInitializedAsync()
    {
        await LoadRoles();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            dotnetHelper = DotNetObjectReference.Create(this);

            await JS.InvokeVoidAsync("initRolesSelect", dotnetHelper);
        }
    }

    private async Task LoadRoles()
    {
        RoleDto = await RoleService.GetAllRoles();

        StateHasChanged();
    }

    [JSInvokable]
    public Task UpdateRoles(string[] values)
    {
        UserDto.RoleIds = values.ToList();

        Console.WriteLine("====== ROLES RECUS ======");

        foreach (var role in UserDto.RoleIds)
        {
            Console.WriteLine(role);
        }

        return Task.CompletedTask;
    }

    private async Task OnSaveUser()
    {
        var resp = await UserService.Save(UserDto);

        if (resp?.Success == true)
        {
            UserDto = new UserDto();
            ToastService.ShowSuccess(resp.Message);
            await Task.Delay(3000);
            StateHasChanged();
            NavigationService.GoListeUtilisateur();
        }
        else
        {
            await JS.InvokeVoidAsync("showToast", $"{resp!.Message}", "warning");
            ToastService.ShowError(resp!.Message);
            // 2. Laisser le temps au toast de s'afficher/être visible
            await Task.Delay(3000);
        }
    }

    public void OnCancel()
    {
        NavigationService.GoListeUtilisateur();
    }

 
}
