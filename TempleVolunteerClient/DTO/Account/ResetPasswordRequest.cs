using TempleVolunteerClient.Common;

namespace TempleVolunteerClient
{
    public class ResetPasswordRequest : Audit
    {
        public string EmailAddress { get; set; }
        public string Token { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
