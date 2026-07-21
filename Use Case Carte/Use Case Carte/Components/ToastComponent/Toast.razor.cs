using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Use_Case_Carte.Models;
using Use_Case_Carte.Services;

namespace Use_Case_Carte.Components.ToastComponent
{
    public partial class Toast : ComponentBase, IDisposable
    {
        [Inject]
        public ToastService ToastService { get; set; } = default!;

        protected ToastModel ToastModel = new();

        protected bool Visible;

        protected string CssClass => ToastModel.Type switch
        {
            ToastType.Success => "toast-success",
            ToastType.Error => "toast-error",
            ToastType.Warning => "toast-warning",
            _ => "toast-info"
        };

        protected override void OnInitialized()
        {
            ToastService.OnShow += ShowToast;
        }

        private async void ShowToast(ToastModel toast)
        {
            ToastModel = toast;
            Visible = true;

            await InvokeAsync(StateHasChanged);

            await Task.Delay(toast.Duration);

            Visible = false;

            await InvokeAsync(StateHasChanged);
        }

        protected void Close()
        {
            Visible = false;
        }

        public void Dispose()
        {
            ToastService.OnShow -= ShowToast;
        }
    }
}