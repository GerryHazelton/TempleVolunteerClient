
using Microsoft.AspNetCore.Mvc.Rendering;
using TempleVolunteerClient;

namespace TempleVolunteerClient
{
    [Serializable]
    public class CommitteeViewModel : Audit
    {
        public int CommitteeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }

        public ICollection<SelectListItem> Areas { get; set; }
        public int[] AreaIds { get; set; }

        public ICollection<SelectListItem> Staff { get; set; }
        public int[] StaffIds { get; set; }

        public CommitteeViewModel()
        {
            this.Areas = new HashSet<SelectListItem>();
            this.Staff = new HashSet<SelectListItem>();
        }
    }
}
