using Microsoft.AspNetCore.Components;
using Use_Case_Carte.Services;

namespace Use_Case_Carte.Components.Skeletor
{
    public partial class Header : ComponentBase{

        [Inject]
        private AuthService AuthService {get; set;} = default!;

        public async Task OnLogOut()
{
    Console.WriteLine("Logout cliqué");

    await AuthService.Logout();
}
        
    }
}