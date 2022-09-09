using TempleVolunteerClient.Common;

namespace TempleVolunteerClient
{
    public class CategoryRequest : Audit
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
    }
}
