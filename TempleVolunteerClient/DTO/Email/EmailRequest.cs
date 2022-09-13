namespace TempleVolunteerClient
{
    public class EmailRequest : Audit
    {
        public string EmailAddress { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string Environment { get; set; }
    }
}
