using Microsoft.JSInterop;

namespace Use_Case_Carte.Services
{
    public class NotificationInAppService
    {
        private readonly IJSRuntime _js;

        public NotificationInAppService(IJSRuntime js)
        {
            _js = js;
        }

        public ValueTask Success(string message)
            => _js.InvokeVoidAsync("toastrHelper.success", message);

        public ValueTask Error(string message)
            => _js.InvokeVoidAsync("toastrHelper.error", message);

        public ValueTask Warning(string message)
            => _js.InvokeVoidAsync("toastrHelper.warning", message);
    }
}
