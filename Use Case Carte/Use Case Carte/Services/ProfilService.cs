using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Use_Case_Carte.Components.Layout;
using Use_Case_Carte.Models;

namespace Use_Case_Carte.Services
{
    public class ProfilService : BaseApiService
    {
        private readonly IJSRuntime _js;

        private SafeJs _safeJs;

        public ProfilService(
            HttpClient http,
            ILocalStorageService storage,
            IJSRuntime js,
            SafeJs safeJs,
            NavigationManager navigation
        )
            : base(http, storage, navigation)
        {
            _js = js;
            _safeJs = safeJs;
        }

        // Retour standardisé : ApiResponse<ProfilModel>
        public async Task<ApiResponse<ProfilModel>> Save(ProfilModel request)
        {
            try
            {
                await AddAuthHeader();

                await _js.InvokeVoidAsync("toggleOnLoaderAndToast");

                var response = await _http.PostAsJsonAsync("api/profil/save", request);

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProfilModel>>();

                if (result == null)
                    throw new Exception("Réponse invalide du serveur.");

                if (result.Success)
                {
                    await _js.InvokeVoidAsync("toggleOffLoaderAndToast");
                    return result;
                }
                else
                {
                    await _js.InvokeVoidAsync("toggleOffLoaderAndToast");
                    return result;
                }

            }
            finally
            {
                await _safeJs.SafeJsUtilities("toggleOffLoaderAndToast");
            }
        }

        public async Task<IEnumerable<ProfilModel>> GetAllProfils()
        {
            await AddAuthHeader();

            await _js.InvokeVoidAsync("toggleOnLoaderAndToast");

            var response = await _http.GetAsync("api/profils");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<
                    ApiResponse<IEnumerable<ProfilModel>>
                >();
                await _js.InvokeVoidAsync("toggleOffLoaderAndToast");

                return result?.Data ?? new List<ProfilModel>();
            }
            else
            {
                await _js.InvokeVoidAsync("toggleOffLoaderAndToast");

                throw new Exception("Erreur lors de la récupération des profils.");
            }
        }

        public async Task<ApiResponse<ProfilModel>?> UpdateProfil(Guid id, ProfilModel profilModel)
        {
            try
            {
                await AddAuthHeader();

                await _js.InvokeVoidAsync("toggleOnLoaderAndToast");

                var response = await _http.PutAsJsonAsync($"api/profil/{id}", profilModel);

                Console.WriteLine($"response {response.IsSuccessStatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    await _js.InvokeVoidAsync("toggleOffLoaderAndToast");

                    return await response.Content.ReadFromJsonAsync<ApiResponse<ProfilModel>>();
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await _js.InvokeVoidAsync("toggleOffLoaderAndToast");

                    throw new Exception(
                        $"Erreur lors de la Modification du profil. Le profil à modifier n'existe pas."
                    );
                }

                await _js.InvokeVoidAsync("toggleOffLoaderAndToast");

                throw new Exception($"Erreur lors de la Modification du profil .");
            }
            catch (Exception ex)
            {
                await _js.InvokeVoidAsync("toggleOffLoaderAndToast");

                throw new Exception($"Erreur lors de la Modification du profil {ex.Message}.");
            }
        }

        public async Task<ProfilModel?> GetById(Guid id)
        {
            await AddAuthHeader();

            var response = await _http.GetAsync($"api/profil/{id}");

            Console.WriteLine($"Status : {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProfilModel>>();

                return result?.Data;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            throw new Exception("Erreur lors de la récupération du profil.");
        }

        public async Task Logout()
        {
            await _storage.RemoveItemAsync("authToken");
            await _storage.RemoveItemAsync("refreshToken");
        }

        public async Task<string?> GetToken()
        {
            return await _storage.GetItemAsync<string>("authToken");
        }
    }
}
