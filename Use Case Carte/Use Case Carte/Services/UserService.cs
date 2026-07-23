using System.Text.Json;
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
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

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

        public async Task<ApiResponse<string>?> UpdateUser(Guid id, UpdateUserDto updateUserDto)
        {
            await _js.InvokeVoidAsync("toggleOnLoaderAndToast");

            try
            {
                await AddAuthHeader();

                var response = await _http.PutAsJsonAsync($"api/users/{id}", updateUserDto);
                var content = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"STATUS UpdateUser : {response.StatusCode}");
                Console.WriteLine($"RESPONSE UpdateUser : {content}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Le profil à modifier n'existe pas.",
                        Errors = new List<string> { content },
                    };
                }

                try
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<string>>(
                        content,
                        _jsonOptions
                    );
                    return result
                        ?? new ApiResponse<string>
                        {
                            Success = false,
                            Message = "Réponse du serveur vide ou invalide",
                            Errors = new List<string> { content },
                        };
                }
                catch (JsonException jex)
                {
                    Console.WriteLine("Erreur de parsing JSON : " + jex.Message);
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = response.IsSuccessStatusCode
                            ? "Réponse du serveur invalide"
                            : $"Erreur HTTP {(int)response.StatusCode} : {response.ReasonPhrase}",
                        Errors = new List<string> { content },
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Erreur inattendue : " + ex.Message,
                    Errors = new List<string> { ex.ToString() },
                };
            }
            finally
            {
                await _js.InvokeVoidAsync("toggleOffLoaderAndToast");
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
