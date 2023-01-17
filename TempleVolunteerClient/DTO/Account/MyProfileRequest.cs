
namespace TempleVolunteerClient
{
    public class MyProfileRequest : Audit
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string? Address2 { get; set; }
        public string City { get; set; }
        public string? State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public string EmailAddress { get; set; }
        public int PropertyId { get; set; }
        public string? StaffFileName { get; set; }
        public byte[]? StaffImage { get; set; }
        public int[]? CredentialIds { get; set; }
        public bool RemovePhoto { get; set; }
    }
}
