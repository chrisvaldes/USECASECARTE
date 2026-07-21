using Microsoft.AspNetCore.Components;
using Use_Case_Carte.Components.Route;

namespace Use_Case_Carte.Components.Shared;

public partial class SharedHeader : ComponentBase
{
    [Inject]
    NavigationService NavigationService { get; set; } = default!;

    [Parameter]
    public string CurrentPage { get; set; } = string.Empty;

    [Parameter]
    public string PreviousPage { get; set; } = string.Empty;

    [Parameter]
    public EventCallback OnCancel { get; set; }
}
