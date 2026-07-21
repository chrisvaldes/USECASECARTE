// using Blazored.LocalStorage;
// using Microsoft.AspNetCore.Components.Authorization;
// using System.Security.Claims;
// using System.Text.Json;

// namespace Use_Case_Carte.Services
// {
//     public class JwtAuthenticationStateProvider : AuthenticationStateProvider
//     {
//         private readonly ILocalStorageService _storage;

//         public JwtAuthenticationStateProvider(ILocalStorageService storage)
//         {
//             _storage = storage;
//         }

//         public override async Task<AuthenticationState> GetAuthenticationStateAsync()
//         {
//             var token = await _storage.GetItemAsync<string>("authToken");

//             if (string.IsNullOrWhiteSpace(token))
//             {
//                 return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
//             }

//             if (IsTokenExpired(token))
//             {
//                 await _storage.RemoveItemAsync("authToken");
//                 return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
//             }

//             var identity = new ClaimsIdentity(ParseClaims(token), "jwt");

//             return new AuthenticationState(new ClaimsPrincipal(identity));
//         }

//         private bool IsTokenExpired(string token)
//         {
//             var payload = token.Split('.')[1];
//             var jsonBytes = ParseBase64WithoutPadding(payload);

//             var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

//             if (!keyValuePairs.TryGetValue("exp", out var exp))
//                 return true;

//             var expDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp.ToString()));

//             return expDate < DateTimeOffset.UtcNow;
//         }

//         public void NotifyUserChanged()
//         {
//             NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
//         }

//         private IEnumerable<Claim> ParseClaims(string jwt)
//         {
//             var claims = new List<Claim>();

//             var payload = jwt.Split('.')[1];

//             var jsonBytes = ParseBase64WithoutPadding(payload);

//             var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

//             foreach (var kvp in keyValuePairs)
//             {
//                 if (kvp.Value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
//                 {
//                     foreach (var item in jsonElement.EnumerateArray())
//                     {
//                         claims.Add(new Claim(kvp.Key, item.ToString()));
//                     }
//                 }
//                 else
//                 {
//                     claims.Add(new Claim(kvp.Key, kvp.Value.ToString()));
//                 }
//             }

//             return claims;
//         }

//         private byte[] ParseBase64WithoutPadding(string base64)
//         {
//             base64 = base64.Replace('-', '+').Replace('_', '/');

//             switch (base64.Length % 4)
//             {
//                 case 2: base64 += "=="; break;
//                 case 3: base64 += "="; break;
//             }

//             return Convert.FromBase64String(base64);
//         }
//     }
// }
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace Use_Case_Carte.Services
{
    public class JwtAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _storage;
        private readonly NavigationManager _navigation;

        public JwtAuthenticationStateProvider(
            ILocalStorageService storage,
            NavigationManager navigation)
        {
            _storage = storage;
            _navigation = navigation;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _storage.GetItemAsync<string>("authToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                RedirectToLoginIfNeeded();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            if (IsTokenExpired(token))
            {
                await _storage.RemoveItemAsync("authToken");
                RedirectToLoginIfNeeded();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var identity = new ClaimsIdentity(ParseClaims(token), "jwt");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        private void RedirectToLoginIfNeeded()
        {
            var currentPath = new Uri(_navigation.Uri).AbsolutePath.TrimEnd('/');

            // Ne redirige pas si on est déjà sur "/" (évite une boucle infinie)
            if (!string.IsNullOrEmpty(currentPath))
            {
                _navigation.NavigateTo("/", forceLoad: true);
            }
        }

        private bool IsTokenExpired(string token)
        {
            var payload = token.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs is null || !keyValuePairs.TryGetValue("exp", out var exp))
                return true;

            var expDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp.ToString()!));
            return expDate < DateTimeOffset.UtcNow;
        }

        public void NotifyUserChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void NotifyUserLogout()
        {
            NotifyAuthenticationStateChanged(
                Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())))
            );
        }

        private IEnumerable<Claim> ParseClaims(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs is null) return claims;

            foreach (var kvp in keyValuePairs)
            {
                if (kvp.Value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in jsonElement.EnumerateArray())
                        claims.Add(new Claim(kvp.Key, item.ToString()));
                }
                else
                {
                    claims.Add(new Claim(kvp.Key, kvp.Value.ToString()!));
                }
            }

            return claims;
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