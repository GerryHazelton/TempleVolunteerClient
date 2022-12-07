
using TempleVolunteerClient.Common;

namespace TempleVolunteerClient
{
    public class RegisterRequest : Audit
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string? Address2 { get; set; }
        public string City { get; set; }
        public string? State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public bool FirstAid { get; set; }
        public bool CPR { get; set; }
        public bool Kriyaban { get; set; }
        public bool LessonStudent { get; set; }
        public bool AcceptTerms { get; set; }

        // testing only
        public bool? CanSendMessages { get; set; }
        public bool? CanViewDocuments { get; set; }
        public bool? EmailConfirmed { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? VerifiedDate { get; set; }
    }
}
