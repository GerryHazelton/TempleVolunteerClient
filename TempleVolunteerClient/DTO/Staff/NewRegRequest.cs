using System.Data;

namespace TempleVolunteerClient
{
    public class NewRegRequest : Audit
    {
        public int StaffId { get; set; }
        public bool Approve { get; set; }
    }
}
