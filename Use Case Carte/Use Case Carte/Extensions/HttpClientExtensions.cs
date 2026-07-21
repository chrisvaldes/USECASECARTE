using Blazored.LocalStorage;

namespace Use_Case_Carte.Extensions
{
    public static class HttpClientExtensions
    {
        public static async Task AddJwt(this HttpClient client, ILocalStorageService storage)
        {
            var token = await storage.GetItemAsync<string>("authToken");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}
