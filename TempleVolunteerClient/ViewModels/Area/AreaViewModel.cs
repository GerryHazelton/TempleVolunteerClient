
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using TempleVolunteerClient;

namespace TempleVolunteerClient
{
    [Serializable]
    public class AreaViewModel : Audit
    {
        public int AreaId { get; set; }

        [Display(Name = "Name")] 
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Note")]
        public string? Note { get; set; }

        [Display(Name = "Supply Items")]
        public ICollection<SelectListItem>? SupplyItems { get; set; }

        public int[]? SupplyItemIds { get; set; }

        [Display(Name = "Event Tasks")]
        public ICollection<SelectListItem>? EventTasks { get; set; }

        public int[]? EventTaskIds { get; set; }
    }
}
