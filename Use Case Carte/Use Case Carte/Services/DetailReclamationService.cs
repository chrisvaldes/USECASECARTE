using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Use_Case_Carte.Components.Layout;
using Use_Case_Carte.Models;

namespace Use_Case_Carte.Services
{
    public class DetailReclamationService : BaseApiService
    {
        private SafeJs _safeJs;
        private readonly IJSRuntime _js;

        public DetailReclamationService(
            SafeJs safeJs,
            HttpClient http,
            ILocalStorageService storage,
            IJSRuntime js
        )
            : base(http, storage)
        {
            _js = js;
            _safeJs = safeJs;
        }

        public async Task<CustomerBilling?> GetAllFacturation(
            string ncpf,
            DateTime debut,
            DateTime? fin = null
        )
        {
            await AddAuthHeader();
            await _js.InvokeVoidAsync("toggleOnLoaderAndToast");

            try
            {
                var query =
                    $"api/customer-billing?ncpf={Uri.EscapeDataString(ncpf)}&debut={debut:o}";

                if (fin.HasValue)
                {
                    query += $"&fin={fin.Value:o}";
                }

                var response = await _http.GetAsync(query);
                var content = await response.Content.ReadAsStringAsync();

                Console.WriteLine("===== JSON RETOUR API =====");
                Console.WriteLine(content);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(
                        $"Erreur lors de la récupération de la facturation : {response.StatusCode}\n{content}"
                    );
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var result = JsonSerializer.Deserialize<ApiResponse<CustomerBilling>>(
                    content,
                    options
                );

                if (result == null)
                {
                    Console.WriteLine(
                        "La désérialisation de ApiResponse<CustomerBilling> a échoué."
                    );
                    return new CustomerBilling();
                }

                Console.WriteLine($"Success : {result.Success}");
                Console.WriteLine($"Message : {result.Message}");

                if (result.Data != null)
                {
                    Console.WriteLine($"BkmvtiResults : {result.Data.BkmvtiResults.Count}");
                    Console.WriteLine($"Synthese : {result.Data.CustomerBillingSynthese.Count}");
                }
                else
                {
                    Console.WriteLine("Data est NULL");
                }

                return result.Data ?? new CustomerBilling();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex}");
                throw;
            }
            finally
            {
                await _js.InvokeVoidAsync("toggleOffLoaderAndToast");
            }
        }
    }
}
