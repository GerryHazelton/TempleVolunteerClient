namespace TempleVolunteerClient
{
    public class TokenRequest
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public bool IsAdmin { get; set; }
        public int StaffId { get; set; }
        public int PropertyId { get; set; }
    }
}
