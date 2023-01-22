using System.Text.Json.Serialization;

namespace TempleVolunteerClient
{
    public class AuthenticateResponse
    {
        public int StaffId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Address { get; set; }
        public string? Address2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? EmailAddress { get; set; }
        public string? Password { get; set; }
        public string? PhoneNumber { get; set; }
        public bool FirstAid { get; set; }
        public bool CPR { get; set; }
        public bool Kriyaban { get; set; }
        public bool LessonStudent { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool AcceptedTerms { get; set; }
        public string? JwtToken { get; set; }

        [JsonIgnore] // refresh token is returned in http only cookie
        public string? RefreshToken { get; set; }

        public static implicit operator AuthenticateResponse(string v)
        {
            throw new NotImplementedException();
        }
    }
}
