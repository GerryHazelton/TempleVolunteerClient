
using System.ComponentModel.DataAnnotations;
using TempleVolunteerClient.Common;

namespace TempleVolunteerClient
{
    public class EventTaskViewModel : Audit
    {
        public EventTaskViewModel()
        {
        }

        public int TaskId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
    }

}