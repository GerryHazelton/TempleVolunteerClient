using TempleVolunteerClient.Common;

namespace TempleVolunteerClient
{
    public class SupplyItemRequest : Audit
    {
        public int SupplyItemId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
        public int Quantity { get; set; }
        public string BinNumber { get; set; }
        public string SupplyItemFileName { get; set; }
        public byte[]? SupplyItemImage { get; set; }
    }
}
