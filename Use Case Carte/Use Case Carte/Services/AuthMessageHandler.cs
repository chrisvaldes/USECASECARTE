﻿using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;

namespace Use_Case_Carte.Services
{
    public class AuthMessageHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;
        private readonly NavigationManager _navigation;

        public AuthMessageHandler(ILocalStorageService localStorage, NavigationManager navigation)
        {
            _localStorage = localStorage;
            _navigation = navigation;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");

            // Vérifier si le token est expiré AVANT d'envoyer la requête
            if (!string.IsNullOrEmpty(token) && IsTokenExpired(token))
            {
                await ClearTokenAndRedirect();
                return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            }

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            // Envoyer la requête
            var response = await base.SendAsync(request, cancellationToken);

            // Si la réponse est 401 (Unauthorized), le token est probablement expiré
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var storedToken = await _localStorage.GetItemAsync<string>("authToken");
                if (!string.IsNullOrWhiteSpace(storedToken))
                {
                    await ClearTokenAndRedirect();
                }
            }

            return response;
        }

        private async Task ClearTokenAndRedirect()
        {
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("refreshToken");

            // Éviter une boucle de redirection si on est déjà sur la page de login
            var currentPath = new Uri(_navigation.Uri).AbsolutePath.TrimEnd('/');
            if (!string.IsNullOrEmpty(currentPath) && currentPath != "/")
            {
                _navigation.NavigateTo("/", forceLoad: true);
            }
        }

        private bool IsTokenExpired(string token)
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

        private byte[] ParseBase64WithoutPadding(string base64)
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
