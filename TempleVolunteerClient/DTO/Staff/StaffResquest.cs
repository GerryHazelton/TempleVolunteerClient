using System.Data;

namespace TempleVolunteerClient
{
    public class StaffRequest : Audit
    {
        public int StaffId { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? Address { get; set; }
        public string? Address2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public int? RoleId { get; set; }
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public bool? AcceptTerms { get; set; }
        public string? Note { get; set; }
        public bool? CanViewDocuments { get; set; }
        public bool? CanSendMessages { get; set; }
        public bool? NewRegistration { get; set; }
        public bool? Approve { get; set; }
        public bool? RememberMe { get; set; }
        public bool? EmailConfirmed { get; set; }
        public DateTime? EmailConfirmedDate { get; set; }
        public bool? IsLockedOut { get; set; }
        public int? LoginAttempts { get; set; }
        public bool? UnlockUser { get; set; }
        public int[]? RoleIds { get; set; }
        public string? StaffFileName { get; set; }
        public byte[]? StaffImage { get; set; }
        public int[]? CredentialIds { get; set; }
        public bool RemovePhoto { get; set; }
    }
}
