using TempleVolunteerClient.Common;

namespace TempleVolunteerClient
{
    public class RefreshTokenRequest : Audit
    {
        public int UserId { get; set; }
        public string RefreshToken { get; set; }
    }
}
