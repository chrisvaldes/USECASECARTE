using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Use_Case_Carte.Components.Route;
using Use_Case_Carte.Models;
using Use_Case_Carte.Services;

namespace Use_Case_Carte.Components.Pages.Synthese
{
    public partial class Synthese : ProtectedPageBase
    {
        protected override string[] RequiredPermissions => new[] { "BKMVTI_CONSULTER", "TYPEMAG" };

        private readonly ILogger<Synthese> _logger;

        [Inject]
        private NavigationService NavigationService { get; set; } = default!;

        [Inject]
        private TypeMagService TypeMagService { get; set; } = default!;

        protected TypeMagWithSyntheseDto typeMagWithSyntheseDto = new();

        [Parameter]
        public Guid Id { get; set; }

        [Inject]
        IJSRuntime JS { get; set; } = default!;

        public Synthese(ILogger<Synthese> logger)
        {
            _logger = logger;
        }

        protected override async Task OnParametersSetAsync()
        {
            Console.WriteLine($"OnParametersSet : {Id}");

            if (Id != Guid.Empty)
            {
                typeMagWithSyntheseDto = await TypeMagService.GetSynthseMag(Id);

                Console.WriteLine("Synthèse chargée");
            }
        }

        private bool loaded;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !loaded)
            {
                loaded = true;

                typeMagWithSyntheseDto = await TypeMagService.GetSynthseMag(Id);

                StateHasChanged();
            }
        }

        public void GoToListGestionMAG()
        {
            NavigationService.GoGestionMAG();
        }

        private async Task OnCancel()
        {
            NavigationService.GoGestionMAG();
            await Task.CompletedTask;
        }
    }
}
