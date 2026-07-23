﻿using System;
using System.Net.Http.Headers;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;

namespace Use_Case_Carte.Services
{
    public abstract class BaseApiService
    {
        protected readonly HttpClient _http;
        protected readonly ILocalStorageService _storage;
        protected readonly NavigationManager _navigation;

        protected BaseApiService(HttpClient http, ILocalStorageService storage, NavigationManager navigation)
        {
            _http = http;
            _storage = storage;
            _navigation = navigation;
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

                // Vérifier si le token est expiré AVANT d'ajouter l'en-tête
                if (IsTokenExpired(token))
                {
                    await ClearTokenAndRedirect();
                    throw new UnauthorizedAccessException("Token expiré. Veuillez vous reconnecter.");
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
            catch (UnauthorizedAccessException)
            {
                // Relancer l'exception d'autorisation pour qu'elle soit traitée par l'appelant
                throw;
            }
            catch (Exception ex)
            {
                // En cas d'erreur inattendue, supprimer l'en-tête et logger.
                _http.DefaultRequestHeaders.Authorization = null;
                Console.WriteLine($"Erreur dans AddAuthHeader : {ex.Message}");
            }
        }

        protected async Task ClearTokenAndRedirect()
        {
            await _storage.RemoveItemAsync("authToken");
            await _storage.RemoveItemAsync("refreshToken");

            // Éviter une boucle de redirection si on est déjà sur la page de login
            var currentPath = new Uri(_navigation.Uri).AbsolutePath.TrimEnd('/');
            if (!string.IsNullOrEmpty(currentPath) && currentPath != "/")
            {
                _navigation.NavigateTo("/", forceLoad: true);
            }
        }

        protected bool IsTokenExpired(string token)
        {
            try
            {
                var payload = token.Split('.')[1];
                var jsonBytes = ParseBase64WithoutPadding(payload);
                var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

                if (keyValuePairs is null || !keyValuePairs.TryGetValue("exp", out var exp))
                    return true;

                var expDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp.ToString()!));
                return expDate < DateTimeOffset.UtcNow;
            }
            catch
            {
                // Si on ne peut pas décoder le token, le considérer comme expiré
                return true;
            }
        }

        protected byte[] ParseBase64WithoutPadding(string base64)
        {
            base64 = base64.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
