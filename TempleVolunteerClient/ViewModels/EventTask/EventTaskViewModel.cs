
using System.ComponentModel.DataAnnotations;
using TempleVolunteerClient.Common;

namespace TempleVolunteerClient
{
    public class EventTaskViewModel : Audit
    {
        public int EventTaskId { get; set; }
        
        [Display(Name = "Event Tasks")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Note")]
        public string Note { get; set; }
    }
}