using System;
using System.Collections.Generic;
using System.Text;

namespace TempleVolunteerClient.Common
{
    public class ServiceResponse
    {
        public object? Data { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; } = null;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsAdmin { get; set; }
        public int StaffId { get; set; }
        public int RoleId { get; set; }
        public string? EmailAddress { get; set; }
        public int LoginAttempts { get; set; }
        public bool IsLockedOut { get; set; }
        public bool RememberMe { get; set; }
        public string? PasswordHash { get; set; }
        public string? VerificationToken { get; set; }
    }
}
