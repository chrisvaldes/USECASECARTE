using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.JSInterop;
using Use_Case_Carte.Models;

namespace Use_Case_Carte.Services
{
    public class RoleService : BaseApiService
    {
        private readonly ILogger<RoleService> _logger;

        private readonly IJSRuntime _js;

        public RoleService(
            HttpClient http,
            ILocalStorageService storage,
            ILogger<RoleService> logger,
            IJSRuntime js
        )
            : base(http, storage)
        {
            _logger = logger;
            _js = js;
        }

        public async Task<ApiResponse<string>> CreateAsync(CreateRoleDto dto)
        {
            await AddAuthHeader();

            try
            {
                var response = await _http.PostAsJsonAsync("api/Role", dto);
                var content = await response.Content.ReadAsStringAsync();

                // ⚠️ LOG CRITIQUE : regarde ce log en priorité dans ta console
                _logger.LogInformation(
                    "StatusCode={StatusCode} | ContentType={ContentType} | Body={Content}",
                    response.StatusCode,
                    response.Content.Headers.ContentType,
                    content
                );

                // Vérifie si la réponse ressemble à du JSON avant de parser
                var trimmed = content.TrimStart();
                bool looksLikeJson = trimmed.StartsWith("{") || trimmed.StartsWith("[");

                if (!looksLikeJson)
                {
                    _logger.LogWarning("La réponse n'est pas du JSON : {Content}", content);

                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = string.IsNullOrWhiteSpace(content)
                            ? $"Erreur serveur (HTTP {(int)response.StatusCode})"
                            : content,
                    };
                }

                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };

                if (!response.IsSuccessStatusCode)
                {
                    try
                    {
                        var errorResult = System.Text.Json.JsonSerializer.Deserialize<
                            ApiResponse<string>
                        >(content, options);
                        if (errorResult is not null)
                            return errorResult;
                    }
                    catch (System.Text.Json.JsonException)
                    {
                        // Format différent (ValidationProblemDetails) -> fallback ci-dessous
                    }

                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(content);
                        var messages = new List<string>();

                        if (doc.RootElement.TryGetProperty("errors", out var errorsElement))
                        {
                            if (errorsElement.ValueKind == System.Text.Json.JsonValueKind.Object)
                            {
                                foreach (var field in errorsElement.EnumerateObject())
                                foreach (var msg in field.Value.EnumerateArray())
                                    messages.Add(msg.GetString() ?? "");
                            }
                            else if (
                                errorsElement.ValueKind == System.Text.Json.JsonValueKind.Array
                            )
                            {
                                foreach (var msg in errorsElement.EnumerateArray())
                                    messages.Add(msg.GetString() ?? "");
                            }
                        }

                        return new ApiResponse<string>
                        {
                            Success = false,
                            Message = doc.RootElement.TryGetProperty("title", out var t)
                                ? (t.GetString() ?? "Erreur de validation")
                                : "Erreur de validation",
                            Errors = messages,
                        };
                    }
                    catch (Exception parseEx)
                    {
                        _logger.LogWarning(parseEx, "Impossible de parser la réponse d'erreur");
                        return new ApiResponse<string> { Success = false, Message = content };
                    }
                }

                var result = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<string>>(
                    content,
                    options
                );
                return result!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur CreateAsync (Role)");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Erreur technique : {ex.Message}",
                };
            }
        }

        public async Task<ApiResponse<string>> UpdateAsync(Guid roleId, CreateRoleDto dto)
        {
            await AddAuthHeader();

            try
            {
                var response = await _http.PutAsJsonAsync($"api/Role/{roleId}", dto);

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Role UPDATE failed: {response.StatusCode} - {errorText}");
                    return result!;
                }

                return result!;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur UpdateAsync (Role): {ex.Message}");
                throw new Exception($"Erreur : {ex.Message}");
            }
        }

        public async Task<IEnumerable<RoleDto>> GetAllRoles()
        {
            await AddAuthHeader();
            await _js.InvokeVoidAsync("toggleOnLoaderAndToast");

            try
            {
                var response = await _http.GetAsync("api/role");
                var content = await response.Content.ReadAsStringAsync();

                _logger.LogInformation(
                    "StatusCode={StatusCode} | Body={Content}",
                    response.StatusCode,
                    content
                );

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Erreur lors de la récupération des roles.");
                }

                var result = JsonSerializer.Deserialize<IEnumerable<RoleDto>>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                return result ?? new List<RoleDto>();
            }
            finally
            {
                await _js.InvokeVoidAsync("toggleOffLoaderAndToast");
            }
        }
    }
}
