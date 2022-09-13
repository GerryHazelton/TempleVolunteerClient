using TempleVolunteerClient.Common;

namespace TempleVolunteerClient
{
    public class VerifyEmailAddressRequest : Audit
    {
        public string? EmailAddress { get; set; }
        public string? Token { get; set; }
        public int StaffId { get; set; }
    }
}
