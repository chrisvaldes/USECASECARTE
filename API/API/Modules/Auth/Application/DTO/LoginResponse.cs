using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Application.DTO
{
    public class LoginResponse
    {
        public string? Token { get; set; } = default!;
        public string RefreshToken { get; set; }
        public DateTime Expiration { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public string? RedirectUrl { get; set; }
        public string? Exception { get; set; }
    }
}
