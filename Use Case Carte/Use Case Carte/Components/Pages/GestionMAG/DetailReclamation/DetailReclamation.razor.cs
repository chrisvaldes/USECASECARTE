using Microsoft.AspNetCore.Components;
using Use_Case_Carte.Components.Route;
using Use_Case_Carte.Models;
using Use_Case_Carte.Services;

namespace Use_Case_Carte.Components.Pages.GestionMAG.DetailReclamation
{
    public partial class DetailReclamation : ProtectedPageBase
    {
        protected override string[] RequiredPermissions => new[] { "BKMVTI_CONSULTER", "TYPEMAG" };

        [Inject]
        protected DetailReclamationService detailReclamationService { get; set; } = default!;

        [Inject]
        NavigationService NavigationService { get; set; } = default!;
 
        protected CustomerBilling customerBilling { get; set; } = new();
        protected InputBilling InputBilling { get; set; } = new();
        protected bool isLoading { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await GetCustomerBilling();
        }

        public async Task GetCustomerBilling()
        {
            if (string.IsNullOrWhiteSpace(InputBilling.NumeroCompte))
            {
                Console.WriteLine($"============>>>>>>>>>>^^^^^^^^^^^^data 23 : {InputBilling.NumeroCompte}, {InputBilling.Debut}");

                return;
                
            }

            isLoading = true;
            try
            { 
                var result = await detailReclamationService.GetAllFacturation(
                    InputBilling.NumeroCompte!,
                    InputBilling.Debut,
                    InputBilling.Fin
                );

                customerBilling = result ?? new CustomerBilling();
            }
            finally
            {
                isLoading = false;
            }
        }

        public async Task Submit()
        {
            // À compléter selon la logique métier de soumission de réclamation
            await Task.CompletedTask;
        }

        private async Task OnCancel()
        {
            NavigationService.GoGestionMAG();
            await Task.CompletedTask;
        }
    }
}
