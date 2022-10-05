
using TempleVolunteerClient;

namespace TempleVolunteerClient
{
    public class AreaRequest : Audit
    {
        public int AreaId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
        public bool SupplyItemsAllowed { get; set; }
        public int[] SupplyItemIds { get; set; }
    }
}
