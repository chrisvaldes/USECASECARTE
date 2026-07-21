using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Use_Case_Carte.Components.Layout
{
    public partial class MainLayout : LayoutComponentBase, IDisposable
    {
        [Inject]
        private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

        private System.Threading.Timer? _timer;

        protected override void OnInitialized()
        {
            _timer = new System.Threading.Timer(async _ =>
            {
                await InvokeAsync(async () =>
                {
                    await AuthStateProvider.GetAuthenticationStateAsync();
                });
            }, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}