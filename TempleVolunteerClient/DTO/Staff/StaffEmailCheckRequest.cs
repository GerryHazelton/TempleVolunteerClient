namespace TempleVolunteerClient
{
    public class StaffEmailCheckRequest : Audit
    {
        public int StaffId { get; set; }
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? EmailAddress { get; set; }
    }
}
