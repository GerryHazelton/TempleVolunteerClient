
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using TempleVolunteerClient.Common;

namespace TempleVolunteerClient
{
    public class EventViewModel : Audit
    {
        public int EventId { get; set; }

        [Display(Name = "Event")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Note")]
        public string? Note { get; set; }

        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Event Types")]
        public ICollection<SelectListItem>? EventTypes { get; set; }

        public int[]? EventTypeIds { get; set; }
    }

}