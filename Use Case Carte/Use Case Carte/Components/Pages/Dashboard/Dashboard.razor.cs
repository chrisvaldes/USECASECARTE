using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Use_Case_Carte.Models;
using Use_Case_Carte.Services;

namespace Use_Case_Carte.Components.Pages.Dashboard
{
    public partial class Dashboard : ComponentBase
    {
        [Inject]
        private NavigationManager Navigation { get; set; } = default!;

        [Inject]
        private DashboardService DashboardService { get; set; } = default!;

        [Inject]
        private IJSRuntime JS { get; set; } = default!;

        protected DashboardSynthese dashboardSynthese = new();

        // protected override async Task OnAfterRenderAsync(bool firstRender)
        // {
        //     if (firstRender)
        //     {
        //         await JS.InvokeVoidAsync("toggleOnLoaderAndToast");

        //         await LoadSyntheseDashboard();

        //         await JS.InvokeVoidAsync("toggleOffLoaderAndToast");
        //     }
        // }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            await LoadSyntheseDashboard();

            var labels = dashboardSynthese
                .DashboardResult.Select(x => $"{x.Mois}/{x.Annee}")
                .ToArray();

            var montants = dashboardSynthese.DashboardResult.Select(x => x.Montant).ToArray();

            Console.WriteLine(dashboardSynthese.DashboardResult.Count);

            Console.WriteLine(dashboardSynthese.BkmvtiSyntheseDto.Count);

            await JS.InvokeVoidAsync("drawDashboardChart", labels, montants);
        }

        private async Task LoadSyntheseDashboard()
        {
            Console.WriteLine("On load data to display in the dashboard ");
            dashboardSynthese = await DashboardService.GetSynthese();

            StateHasChanged();
        }
    }
}
