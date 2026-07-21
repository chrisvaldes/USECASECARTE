using Blazored.LocalStorage;
using Microsoft.JSInterop;
using Use_Case_Carte.Components.Layout;
using Use_Case_Carte.Models;

namespace Use_Case_Carte.Services
{
    public class UserService : BaseApiService
    {
        private readonly IJSRuntime _js;

        private SafeJs _safeJs;

        public UserService(
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

        public async Task<ApiResponse<UserDto>> Save(UserDto request)
        {
            try
            {
                await AddAuthHeader();
                await _js.InvokeVoidAsync("toggleOnLoaderAndToast");

 
                var response = await _http.PostAsJsonAsync("api/users", request);
                var content = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"StatusCode={response.StatusCode} | Body={content}");

                var result = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<string>>(
                    content,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    }
                );

                if (result == null)
                    throw new Exception("Réponse invalide du serveur.");

                if (!result.Success)
                {
                    // Échec (ex: "Matricule déjà utilisé") — pas de data à récupérer
                    return new ApiResponse<UserDto>
                    {
                        Success = false,
                        Message = result.Message,
                        Errors = result.Errors,
                        Data = null,
                    };
                }

                // Succès : result.Data contient l'ID (string) du nouvel utilisateur.
                // On va chercher l'utilisateur complet, avec ses rôles inclus.
                if (!Guid.TryParse(result.Data, out var newUserId))
                {
                    return new ApiResponse<UserDto>
                    {
                        Success = true,
                        Message = result.Message,
                        Data = null, // ID invalide/inattendu, on ne peut pas recharger
                    };
                }

                var createdUser = await GetById(newUserId);

                return new ApiResponse<UserDto>
                {
                    Success = true,
                    Message = result.Message,
                    Data = createdUser,
                };
            }
            finally
            {
                await _js.InvokeVoidAsync("toggleOffLoaderAndToast");
                await _safeJs.SafeJsUtilities("toggleOffLoaderAndToast");
            }
        }

        public async Task<IEnumerable<UserDto>> GetAllUsers()
        {
            await AddAuthHeader();

            await _js.InvokeVoidAsync("toggleOnLoaderAndToast");

            var response = await _http.GetAsync("api/users");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<
                    ApiResponse<IEnumerable<UserDto>>
                >();
                await _js.InvokeVoidAsync("toggleOffLoaderAndToast");

                return result?.Data ?? new List<UserDto>();
            }
            else
            {
                await _js.InvokeVoidAsync("toggleOffLoaderAndToast");

                throw new Exception("Erreur lors de la récupération des profils.");
            }
        }

        public async Task<ApiResponse<UserDto>?> UpdateUser(Guid id, UserDto profilModel)
        {
            try
            {
                await AddAuthHeader();

                await _js.InvokeVoidAsync("toggleOnLoaderAndToast");

                var response = await _http.PutAsJsonAsync($"api/users/{id}", profilModel);

                Console.WriteLine($"response {response.IsSuccessStatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    await _js.InvokeVoidAsync("toggleOffLoaderAndToast");

                    return await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
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

        public async Task<UserDto?> GetById(Guid id)
        {
            await AddAuthHeader();

            var response = await _http.GetAsync($"api/users/{id}");

            Console.WriteLine($"Status : {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();

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
