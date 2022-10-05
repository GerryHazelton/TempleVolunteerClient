
using System.ComponentModel.DataAnnotations;
using TempleVolunteerClient.Common;

namespace TempleVolunteerClient
{
    public class EventTypeViewModel : Audit
    {
        public int EventTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }

        public ICollection<AreaRequest> Areas { get; set; }

        public EventTypeViewModel()
        {
            this.Areas = new HashSet<AreaRequest>();

        }
    }

}