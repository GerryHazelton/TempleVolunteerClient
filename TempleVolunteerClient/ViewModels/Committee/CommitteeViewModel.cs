
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using TempleVolunteerClient;

namespace TempleVolunteerClient
{
    [Serializable]
    public class CommitteeViewModel : Audit
    {
        public int CommitteeId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Note")]
        public string Note { get; set; }

        [Display(Name = "Area")]
        public ICollection<SelectListItem> Areas { get; set; }
        public int[] AreaIds { get; set; }

        [Display(Name = "Staff")]
        public ICollection<SelectListItem> Staff { get; set; }
        public int[] StaffIds { get; set; }

        public CommitteeViewModel()
        {
            this.Areas = new HashSet<SelectListItem>();
            this.Staff = new HashSet<SelectListItem>();
        }
    }
}
