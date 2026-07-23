using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Use_Case_Carte.Components.Route;
using Use_Case_Carte.Services;

public class ProtectedPageBase : ComponentBase
{
    [Inject]
    protected AuthService AuthService { get; set; } = default!;

    [Inject]
    protected NavigationService NavigationService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        var token = await AuthService.GetToken();

        if (string.IsNullOrWhiteSpace(token) || IsTokenExpired(token))
        {
            await AuthService.Logout();
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
