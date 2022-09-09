namespace TempleVolunteerClient
{
    public class TokenResponse : BaseResponse
    {
        public int StaffId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsAdmin { get; set; }
        public int PropertyId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string PropertyName { get; set; }
    }
}
