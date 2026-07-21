using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;


namespace Use_Case_Carte.Components.Layout
{
    public class SafeJs
    {
        private readonly IJSRuntime _js;

        public SafeJs(IJSRuntime js)
        {
            _js = js;
        }
        public async Task SafeJsUtilities(string identifier, params object[] args){
            try {
                await _js.InvokeVoidAsync(identifier, args);
            }
            catch {
                    // Ignorer les erreurs JS (prerendering, etc.)
            }
        }
    }
}