using Blazored.LocalStorage;
using Microsoft.JSInterop;
using Use_Case_Carte.Components.Layout;
using Use_Case_Carte.Models;

namespace Use_Case_Carte.Services
{
    public class TypeMagService : BaseApiService
    {
        private readonly IJSRuntime _js;

        private SafeJs _safeJs;

        public TypeMagService(
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

        // Retour standardisé : ApiResponse<ProfilModel>
        public async Task<IEnumerable<TypeMag>> GetAllMags()
        {
            await AddAuthHeader();

            var response = await _http.GetAsync("api/typeMag");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<
                    ApiResponse<IEnumerable<TypeMag>>
                >();
                return result?.Data ?? new List<TypeMag>();
            }
            else
            {
                throw new Exception("Erreur lors de la récupération des types de mags.");
            }
        }

        public async Task GetBkmvti(TypeMag typeMag)
        {
            try
            {
                await AddAuthHeader();

                await _js.InvokeVoidAsync("toggleOnLoaderAndToast");

                var response = await _http.GetAsync($"api/bkmvti/Download/{typeMag.Id}");

                if (!response.IsSuccessStatusCode)
                {
                    await _js.InvokeVoidAsync("toggleOffLoaderAndToast");

                    throw new Exception("Erreur de téléchargement.");
                }

                var bytes = await response.Content.ReadAsByteArrayAsync();

                var fileName =
                    response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                    ?? "BKMVTI.txt";

                await _js.InvokeVoidAsync("downloadFile", fileName, Convert.ToBase64String(bytes));

                await _js.InvokeVoidAsync("toggleOffLoaderAndToast");
            }
            catch (Exception ex)
            {
                await _js.InvokeVoidAsync("toggleOffLoaderAndToast");
                throw new Exception($"Erreur lors du téléchargement bkmvti {ex.Message}");
            }
        }

        public async Task GetCarteAReguler(TypeMag typeMag)
        {
            try
            {
                await AddAuthHeader();

                await _js.InvokeVoidAsync("toggleOnLoaderAndToast");

                var response = await _http.GetAsync($"api/carteAReguler/Download/{typeMag.Id}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(await response.Content.ReadAsStringAsync());
                }

                var bytes = await response.Content.ReadAsByteArrayAsync();

                var fileName =
                    response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                    ?? $"BKMBTI_{DateTime.Today:yyyyMMdd}.xlsx";

                await _js.InvokeVoidAsync(
                    "downloadExcelFile",
                    fileName,
                    Convert.ToBase64String(bytes)
                );

                await _js.InvokeVoidAsync("toggleOffLoaderAndToast");
            }
            catch (Exception ex)
            {
                await _js.InvokeVoidAsync("toggleOffLoaderAndToast");
                throw new Exception($"Erreur lors du téléchargement : {ex.Message}");
            }
        }

        public async Task<TypeMagWithSyntheseDto> GetSynthseMag(Guid Id)
        {
            await AddAuthHeader();

            await _js.InvokeVoidAsync("toggleOnLoaderAndToast");

            var response = await _http.GetAsync($"api/synthese/{Id}");

            Console.WriteLine($"id reçu dans le service typemag : {Id}");
            // await _safeJs.SafeJsUtilities("toggleOnLoaderAndToast");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TypeMagWithSyntheseDto>();

                await _js.InvokeVoidAsync("toggleOffLoaderAndToast");

                return result ?? new TypeMagWithSyntheseDto();
            }
            else
            {
                await _js.InvokeVoidAsync("toggleOnLoaderAndToast");

                throw new Exception("Erreur lors de la récupération de la synthese.");
            }
        }
    }
}
