namespace TempleVolunteerClient
{
    public class ForgotPasswordRequest
    {
        public string? EmailAddress { get; set; }
        public string? PostalCode { get; set; }
        public int PropertyId { get; set; }
    }
}
