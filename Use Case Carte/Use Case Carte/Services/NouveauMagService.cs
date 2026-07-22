using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Use_Case_Carte.Components.Layout;
using Use_Case_Carte.Models;

namespace Use_Case_Carte.Services
{
    /// <summary>
    /// Modèle pour désérialiser la réponse d'erreur de validation ASP.NET (ValidationProblemDetails)
    /// </summary>
    public class ValidationErrorResponse
    {
        public string Type { get; set; } = "";
        public string Title { get; set; } = "";
        public int Status { get; set; }
        public Dictionary<string, string[]> Errors { get; set; } = new();
        public string TraceId { get; set; } = "";
    }

    public class NouveauMagService : BaseApiService
    {
        private SafeJs _safeJs;
        private readonly IJSRuntime _js;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public NouveauMagService(
            SafeJs safeJs,
            HttpClient http,
            ILocalStorageService storage,
            IJSRuntime js
        )
            : base(http, storage)
        {
            _js = js;
            _safeJs = safeJs;
        }
 
        public async Task<ApiResponse<bool>> NouveauMag(InputModel request)
        {
            var tempFiles = new List<string>();
            try
            {
                await _js.InvokeVoidAsync("toggleOnLoaderAndToast");
                await AddAuthHeader();

                using var form = new MultipartFormDataContent();

                form.Add(new StringContent(request.TypeMag ?? ""), "TypeMag");
                form.Add(new StringContent(request.StartPeriod.ToString("o")), "StartPeriod");
                form.Add(new StringContent(request.EndPeriod.ToString("o")), "EndPeriod");

                async Task AddFileAsync(IBrowserFile file, string fieldName)
                {
                    if (file == null)
                        return;

                    var tempPath = Path.GetTempFileName();
                    tempFiles.Add(tempPath);

                    await using (var browserStream = file.OpenReadStream(500 * 1024 * 1024))
                    await using (var fileStream = File.Create(tempPath))
                    {
                        await browserStream.CopyToAsync(fileStream);
                    }

                    var streamContent = new StreamContent(File.OpenRead(tempPath));
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(
                        string.IsNullOrWhiteSpace(file.ContentType)
                            ? "application/octet-stream"
                            : file.ContentType
                    );
                    form.Add(streamContent, fieldName, file.Name);
                }

                await AddFileAsync(request.Apprint, "Apprint");
                await AddFileAsync(request.OpenAccount, "OpenAccount");
                await AddFileAsync(request.ActiveAccount, "ActiveAccount");
                await AddFileAsync(request.ActivePackage, "ActivePackage");
                await AddFileAsync(request.CtxAccount, "CtxAccount");
                await AddFileAsync(request.DateLastSouPackEchu, "DateLastSouPackEchu");
                await AddFileAsync(
                    request.AccountHisDebiteByRedevCard,
                    "AccountHisDebiteByRedevCard"
                );

                var response = await _http.PostAsync("api/nouveauMag", form);

                // Une seule lecture du flux : on lit le texte brut, puis on
                // désérialise à partir de la string déjà lue (pas de ReadFromJsonAsync
                // après un ReadAsStringAsync, sinon le flux est déjà consommé).
                var content = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"STATUS API : {response.StatusCode}");
                Console.WriteLine($"RESPONSE API : {content}");

                await _js.InvokeVoidAsync("toggleOffLoaderAndToast");

                try
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<bool>>(
                        content,
                        _jsonOptions
                    );

                    if (result is null)
                    {
                        await _js.InvokeVoidAsync("toggleOffLoaderAndToast");
                        return new ApiResponse<bool>
                        {
                            Success = false,
                            Message = "Réponse du serveur vide ou invalide",
                            Errors = new List<string> { content },
                        };
                    }

                    return result;
                }
                catch (JsonException jex)
                {
                    Console.WriteLine("Erreur de parsing JSON : " + jex.Message);
                    await _js.InvokeVoidAsync("toggleOffLoaderAndToast");
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = response.IsSuccessStatusCode
                            ? "Réponse du serveur invalide"
                            : $"Erreur HTTP {(int)response.StatusCode} : {response.ReasonPhrase}",
                        Errors = new List<string> { content },
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await _js.InvokeVoidAsync("toggleOffLoaderAndToast");
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Erreur inattendue : " + ex.Message,
                    Errors = new List<string> { ex.ToString() },
                };
            }
            finally
            {
                foreach (var f in tempFiles)
                {
                    try
                    {
                        File.Delete(f);
                    }
                    catch
                    { /* ignoré */
                    }
                }
            }
        }
    }
}
