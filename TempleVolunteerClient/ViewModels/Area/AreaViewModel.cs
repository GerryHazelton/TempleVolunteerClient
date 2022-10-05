
using Microsoft.AspNetCore.Mvc.Rendering;
using TempleVolunteerClient;

namespace TempleVolunteerClient
{
    [Serializable]
    public class AreaViewModel : Audit
    {
        public int AreaId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }

        public bool SupplyItemsAllowed { get; set; }
        public ICollection<SelectListItem> SupplyItems { get; set; }
        public int[] SupplyItemIds { get; set; }

        public AreaViewModel()
        {
            this.SupplyItems = new HashSet<SelectListItem>();
        }
    }
}
