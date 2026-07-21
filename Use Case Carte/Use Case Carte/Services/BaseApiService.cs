using System;
using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace Use_Case_Carte.Services
{
    public abstract class BaseApiService
    {
        protected readonly HttpClient _http;
        protected readonly ILocalStorageService _storage;

        protected BaseApiService(HttpClient http, ILocalStorageService storage)
        {
            _http = http;
            _storage = storage;
        }

        protected async Task AddAuthHeader()
        {
            try
            {
                var token = await _storage.GetItemAsync<string>("authToken");

                if (string.IsNullOrWhiteSpace(token))
                {
                    _http.DefaultRequestHeaders.Authorization = null;
                    return;
                }

                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    token
                );
            }
            catch (InvalidOperationException ex)
            {
                // Prerendering (JS interop non disponible) — ne pas interrompre le flux.
                Console.WriteLine($"AddAuthHeader skipped (prerendering): {ex.Message}");
            }
            catch (Exception ex)
            {
                // En cas d'erreur inattendue, supprimer l'en-tête et logger.
                _http.DefaultRequestHeaders.Authorization = null;
                Console.WriteLine($"Erreur dans AddAuthHeader : {ex.Message}");
            }
        }
    }
}
