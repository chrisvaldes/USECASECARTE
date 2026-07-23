using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Use_Case_Carte.Models;

namespace Use_Case_Carte.Services
{
    public class PermissionService : BaseApiService
    {
        public readonly IJSRuntime _js;

        private ILogger<PermissionService> _logger;

        public PermissionService(
            HttpClient http,
            ILocalStorageService storage,
            IJSRuntime js,
            ILogger<PermissionService> logger,
            NavigationManager navigation
        )
            : base(http, storage, navigation)
        {
            _js = js;
            _logger = logger;
        }

        public async Task<ApiResponse<Permission>> PostPermissionAsync(Permission permission)
        {
            await AddAuthHeader();

            try
            {
       
                await _js.InvokeVoidAsync("toggleOnLoaderAndToast");

                var response = await _http.PostAsJsonAsync("api/Permission", permission);

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync();
                    _logger.LogError(
                        $"Permission POST failed: {response.StatusCode} - {errorText}"
                    );
                    throw new Exception(
                        $"Erreur serveur ({(int)response.StatusCode}): impossible d'enregistrer la permission."
                    );
                }

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<Permission>>();

                return result!;
            }
            finally
            {
                await _js.InvokeVoidAsync("toggleOffLoaderAndToast");
            }
        }

        public async Task<List<PermissionTreeDto>> GetAllAsync()
        {
            await AddAuthHeader(); // si AddAuthHeader retourne le token, sinon adapte 

            try
            {
                var result = await _http.GetFromJsonAsync<List<PermissionTreeDto>>("api/permission");
                return result ?? new List<PermissionTreeDto>();
            }
            catch (Exception ex)
            { 
                return new List<PermissionTreeDto>();
            }
        }
    }
}
