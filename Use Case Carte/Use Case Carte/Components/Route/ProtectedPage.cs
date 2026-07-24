using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Use_Case_Carte.Components.Route;
using Use_Case_Carte.Services;

public class ProtectedPageBase : ComponentBase
{
    [Inject]
    protected AuthService AuthService { get; set; } = default!;

    [Inject]
    protected NavigationService NavigationService { get; set; } = default!;

    [Inject]
    protected AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    /// <summary>
    /// Liste des permissions requises pour accéder à cette page.
    /// Surcharger cette propriété dans les pages dérivées.
    /// Exemple : protected override string[] RequiredPermissions => new[] { "UTILISATEUR" };
    /// </summary>
    protected virtual string[] RequiredPermissions => Array.Empty<string>();

    protected override async Task OnInitializedAsync()
    {
        var token = await AuthService.GetToken();

        // Vérifier si le token existe et n'est pas expiré
        if (string.IsNullOrWhiteSpace(token) || IsTokenExpired(token))
        {
            await AuthService.Logout();
            return;
        }

        // Vérifier les permissions si nécessaire
        if (RequiredPermissions.Length > 0)
        {
            var hasPermission = await CheckPermissionsAsync();
            if (!hasPermission)
            {
                NavigationService.GoToAccessDenied();
                return;
            }
        }
    }

    private async Task<bool> CheckPermissionsAsync()
    {
        try
        {
            var state = await AuthStateProvider.GetAuthenticationStateAsync();
            var userPermissions = state.User.Claims
                .Where(c => c.Type == "permission")
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return RequiredPermissions.Any(p => userPermissions.Contains(p));
        }
        catch
        {
            return false;
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
