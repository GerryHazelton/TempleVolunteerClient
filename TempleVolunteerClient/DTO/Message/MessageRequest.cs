using TempleVolunteerClient.Common;

namespace TempleVolunteerClient
{
    public class MessageRequest : Audit
    {
        public int StaffId { get; set; }
        public string To { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string MessageSent { get; set; }
        public int PropertyId { get; set; }
    }
}
