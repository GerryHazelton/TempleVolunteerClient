
using TempleVolunteerClient;

namespace TempleVolunteerClient
{
    public class CommitteeRequest : Audit
    {
        public int CommitteeId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Note { get; set; }
        public int[] AreaIds { get; set; }
        public int[] StaffIds { get; set; }
    }
}
