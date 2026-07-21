// using Blazored.LocalStorage;
// using Microsoft.AspNetCore.Components;
// using Microsoft.JSInterop;
// using System.Net.Http.Json;
// using Use_Case_Carte.Components.Route;
// using Use_Case_Carte.Models.Auth;

// namespace Use_Case_Carte.Services
// {
//     public class AuthService
//     {
//         private readonly HttpClient _http;
//         private readonly ILocalStorageService _storage;
//         private readonly IJSRuntime _js;
//         private readonly NavigationService _navigationService;

//         public AuthService(
//             HttpClient http,
//             ILocalStorageService storage,
//             IJSRuntime js,
//             NavigationService navigationService)
//         {
//             _http = http;
//             _storage = storage;
//             _js = js;
//             _navigationService = navigationService;
//         }

//         public async Task<LoginResponse> Login(LoginRequest request)
//         {

//             await _js.InvokeVoidAsync("toggleOnLoaderAndToast");


//             Console.WriteLine($"===========>> LoginRequest: {request.Username}, {request.Password}");
//             Console.WriteLine($"===========>> adress de base : {_http.BaseAddress}");
//             var response = await _http.PostAsJsonAsync("api/auth/login", request);

//             Console.WriteLine($"---------->> LoginRequest: {request.Username}, {request.Password}");

//             Console.WriteLine($"---------->> Response: {response.StatusCode}");


//             var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

//             Console.WriteLine($"---------->> Response success? : {result!.Success}");
//             if (result?.Success == true)
//             {
//                 await _storage.SetItemAsync("authToken", result.Token);
//                 await _storage.SetItemAsync("refreshToken", result.RefreshToken);
//                 await _js.InvokeVoidAsync("toggleOffLoaderAndToast");
//             }
//             else
//             {
//                 await _js.InvokeVoidAsync("toggleOffLoaderAndToast");
//                 _navigationService.GoLogin();
//             }
//             return result!;

//         }

//         public async Task Logout()
//         {
//             await _storage.RemoveItemAsync("authToken");
//             await _storage.RemoveItemAsync("refreshToken");
//             _navigationService.GoLogin();
//         }

//         public async Task<string?> GetToken()
//         {
//             return await _storage.GetItemAsync<string>("authToken");
//         }
//     }
// }

using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using Use_Case_Carte.Components.Route;
using Use_Case_Carte.Models.Auth;

namespace Use_Case_Carte.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _storage;
        private readonly IJSRuntime _js;
        private readonly NavigationService _navigationService;

        public AuthService(
            HttpClient http,
            ILocalStorageService storage,
            IJSRuntime js,
            NavigationService navigationService)
        {
            _http = http;
            _storage = storage;
            _js = js;
            _navigationService = navigationService;
        }

        public async Task<LoginResponse> Login(LoginRequest request)
        { 

            // Le loader doit s'activer AVANT l'appel HTTP, pas après
            await _js.InvokeVoidAsync("toggleOnLoaderAndToast");

            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/login", request);
 
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
 

                if (result?.Success == true)
                {
                    await _storage.SetItemAsync("authToken", result.Token);
                    await _storage.SetItemAsync("refreshToken", result.RefreshToken);
                }
                else
                {
                    _navigationService.GoLogin();
                }

                return result ?? new LoginResponse { Success = false, Message = "Réponse invalide du serveur" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"---------->> Erreur Login: {ex.Message}");
                return new LoginResponse { Success = false, Message = "Erreur de connexion au serveur" };
            }
            finally
            {
                // Toujours éteindre le loader, qu'il y ait succès, échec ou exception
                await _js.InvokeVoidAsync("toggleOffLoaderAndToast");
            }
        }

        public async Task Logout()
        {
            await _storage.RemoveItemAsync("authToken");
            await _storage.RemoveItemAsync("refreshToken");
            _navigationService.GoLogin();
        }

        public async Task<string?> GetToken()
        {
            return await _storage.GetItemAsync<string>("authToken");
        }
    }
}