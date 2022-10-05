
using System.ComponentModel.DataAnnotations;
using TempleVolunteerClient.Common;

namespace TempleVolunteerClient
{
    public class EventViewModel : Audit
    {
        public int EventId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Note { get; set; }
        public ICollection<EventTypeRequest>? EventTypes { get; set; }

        public EventViewModel()
        {
            this.EventTypes = new HashSet<EventTypeRequest>();
        }
    }

}