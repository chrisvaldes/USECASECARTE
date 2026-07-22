using Microsoft.AspNetCore.Components.Authorization;

 

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