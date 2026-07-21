using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace Use_Case_Carte.Services
{
    public class JwtMessageHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _storage;

        public JwtMessageHandler(ILocalStorageService storage)
        {
            _storage = storage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = await _storage.GetItemAsync<string>("authToken");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
