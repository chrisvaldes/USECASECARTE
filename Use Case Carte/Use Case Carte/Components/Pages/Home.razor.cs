 

using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Use_Case_Carte.Services;
using Use_Case_Carte.Models.Auth;
using Use_Case_Carte.Components.Route;

namespace Use_Case_Carte.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject]
    public ToastService ToastService { get; set; } = default!;

    [Inject]
    private AuthService AuthService { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private NavigationService NavigationService { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    protected LoginRequest loginRequest = new();

    protected async Task LoginAsync()
    {
        try
        {
            var result = await AuthService.Login(loginRequest);

            if (result.Success && !string.IsNullOrWhiteSpace(result.Token))
            {
                // Déterminer la route de redirection en fonction des permissions dans le token
                var redirectUrl = GetRedirectUrlFromToken(result.Token);

                ToastService.ShowSuccess(result.Message);

                await Task.Delay(2000);

                Navigation.NavigateTo(redirectUrl, forceLoad: true);
            }
            else
            {
                ToastService.ShowError(result.Message ?? "Erreur de connexion");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Extrait les permissions du token JWT et retourne la route appropriée.
    /// </summary>
    private string GetRedirectUrlFromToken(string token)
    {
        try
        {
            // Décoder le payload du JWT (partie du milieu)
            var payload = token.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var claims = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (claims is null)
                return  Route.Route.Dashboard;

            // Extraire les permissions
            var permissions = new List<string>();

            // Les permissions peuvent être sous forme de tableau JSON
            if (claims.TryGetValue("permission", out var permValue))
            {
                if (permValue is JsonElement element && element.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in element.EnumerateArray())
                    {
                        permissions.Add(item.GetString() ?? "");
                    }
                }
                else
                {
                    permissions.Add(permValue.ToString() ?? "");
                }
            }

            // Extraire les rôles
            var roles = new List<string>();
            if (claims.TryGetValue(ClaimTypes.Role, out var roleValue))
            {
                if (roleValue is JsonElement roleElement && roleElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in roleElement.EnumerateArray())
                    {
                        roles.Add(item.GetString() ?? "");
                    }
                }
                else
                {
                    roles.Add(roleValue.ToString() ?? "");
                }
            }

            // === LOGIQUE DE REDIRECTION ===

            // 1. Si l'utilisateur a la permission "UTILISATEUR" → Gestion des utilisateurs
            if (permissions.Any(p => p.Equals("UTILISATEUR", StringComparison.OrdinalIgnoreCase)))
            {
                return Route.Route.Utilisateurs;
            }

            // 2. Si l'utilisateur a la permission "TYPEMAG" ou "BKMVTI" → Dashboard
            if (permissions.Any(p => p.Equals("TYPEMAG", StringComparison.OrdinalIgnoreCase)
                                  || p.Equals("BKMVTI", StringComparison.OrdinalIgnoreCase)))
            {
                return  Route.Route.Dashboard;
            }

            // 3. Si l'utilisateur a la permission "BKMVTI_CONSULTER" → Dashboard
            if (permissions.Any(p => p.Equals("BKMVTI_CONSULTER", StringComparison.OrdinalIgnoreCase)))
            {
                return  Route.Route.Dashboard;
            }

            // 4. Par défaut → Dashboard
            return  Route.Route.Dashboard;
        }
        catch
        {
            // En cas d'erreur de décodage, rediriger vers le Dashboard par défaut
            return Route.Route.Dashboard;
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
