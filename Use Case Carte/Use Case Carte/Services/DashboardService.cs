using Blazored.LocalStorage;
using Microsoft.JSInterop;
using Use_Case_Carte.Components.Layout;
using Use_Case_Carte.Models;

namespace Use_Case_Carte.Services
{
    public class DashboardService : BaseApiService
    {
        private readonly IJSRuntime _js;

        private SafeJs _safeJs;

        public DashboardService(
            HttpClient http,
            ILocalStorageService storage,
            IJSRuntime js,
            SafeJs safeJs
        )
            : base(http, storage)
        {
            _js = js;
            _safeJs = safeJs;
        }

        public async Task<DashboardSynthese> GetSynthese()
        {
            await AddAuthHeader();
            await _js.InvokeVoidAsync("toggleOnLoaderAndToast");

            var response = await _http.GetAsync("api/dashboard");

            // await _safeJs.SafeJsUtilities("toggleOnLoaderAndToast");

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadFromJsonAsync<
                    ApiResponse<DashboardSynthese>
                >();
                await _js.InvokeVoidAsync("toggleOffLoaderAndToast");

                return responseData!.Data;
            }
            else
            {
                await _js.InvokeVoidAsync("toggleOffLoaderAndToast");

                throw new Exception("Erreur lors de la récupération de la synthese.");
            }
        }
    }
}
