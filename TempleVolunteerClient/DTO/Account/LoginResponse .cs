namespace TempleVolunteerClient
{
    public class LoginResponse
    {
        public string? UserName { get; set; }
        public string? EmailAddress { get; set; }
        public string? Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
