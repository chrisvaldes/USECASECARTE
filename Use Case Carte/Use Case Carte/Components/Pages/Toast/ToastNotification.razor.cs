using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Use_Case_Carte.Components.Pages.Toast
{
    public partial class ToastNotification : ComponentBase
    {
        private readonly ILogger<ToastNotification> _logger;

        public ToastNotification(ILogger<ToastNotification> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}