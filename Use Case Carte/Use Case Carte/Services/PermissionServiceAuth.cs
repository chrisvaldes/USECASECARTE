using Microsoft.AspNetCore.Components.Authorization;

// public class PermissionServiceAuth
// {
//     private readonly AuthenticationStateProvider _auth;

//     public PermissionServiceAuth(AuthenticationStateProvider auth)
//     {
//         _auth = auth;
//     }

//     public async Task<AuthenticationState> GetAuthenticationState()
//     {
//         return await _auth.GetAuthenticationStateAsync();
//     }


//     public async Task<HashSet<string>> GetPermissions()
//     {
//         var state = await _auth.GetAuthenticationStateAsync();

//         Console.WriteLine("========== AUTHENTICATION STATE ==========");

//         Console.WriteLine($"Utilisateur authentifié : {state.User.Identity?.IsAuthenticated}");
//         Console.WriteLine($"Nom utilisateur : {state.User.Identity?.Name}");

//         Console.WriteLine("========== CLAIMS ==========");

//         foreach (var claim in state.User.Claims)
//         {
//             Console.WriteLine($"Type : {claim.Type} | Value : {claim.Value}");
//         }

//         var permissions = state.User.Claims
//             .Where(c => c.Type == "permission")
//             .Select(c => c.Value)
//             .ToHashSet();

//         return permissions;
//     }


//     public async Task<bool> HasPermission(string permission)
//     {
//         var permissions = await GetPermissions();
//         return permissions.Contains(permission);
//     }
// }

public class PermissionServiceAuth
{
    private readonly AuthenticationStateProvider _auth;

    public PermissionServiceAuth(AuthenticationStateProvider auth)
    {
        _auth = auth;
    }


    public async Task<HashSet<string>> GetPermissions()
{ 
    var state = await _auth.GetAuthenticationStateAsync();

    var permissions = state.User.Claims
        .Where(c => c.Type == "permission")
        .Select(c => c.Value)
        .ToHashSet();


    return permissions;
}
}