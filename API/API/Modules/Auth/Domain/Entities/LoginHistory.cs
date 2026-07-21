using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Domain.Entities
{
    public class LoginHistory
    {
        [Key]
        public long Id { get; set; }

        public Guid? UserId { get; set; }

        public string Username { get; set; } = default!;

        public DateTime LoginDate { get; set; }

        public bool IsSuccess { get; set; }

        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        public string? FailureReason { get; set; }
    }
}
