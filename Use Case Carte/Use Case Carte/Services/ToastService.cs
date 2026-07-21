using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Use_Case_Carte.Models;

namespace Use_Case_Carte.Services
{
    public class ToastService
    {
        public event Action<ToastModel>? OnShow;

        public void ShowSuccess(string message, string title = "Succes")
        {
            Show(message, title, ToastType.Success);
        }

        public void ShowError(string message, string title = "Erreur")
        {
            Show(message, title, ToastType.Error);
        }

        public void ShowWarning(string message, string title = "Attention")
        {
            Show(message, title, ToastType.Warning);
        }

        public void ShowInfo(string message, string title = "Information")
        {
            Show(message, title, ToastType.Info);
        }

        private void Show(string message, string title, ToastType type)
        {
            OnShow?.Invoke(
                new ToastModel
                {
                    Message = message,
                    Title = title,
                    Type = type,
                }
            );
        }
    }
}
