namespace TempleVolunteerClient
{
    public class ResetPasswordRequest
    {
        public string EmailAddress { get; set; }
        public string Token { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public int PropertyId { get; set; }

    }
}
