namespace TempleVolunteerClient
{
    public class JWT
    {
        public string? Data { get; set; }
        public bool IsLockedOut { get; set; }
        public bool RememberMe { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
