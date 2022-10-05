
using System.ComponentModel.DataAnnotations;
using TempleVolunteerClient.Common;

namespace TempleVolunteerClient
{
    public class MessageViewModel : Audit
    {
        public int StaffId { get; set; }
        public string To { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string MessageSent { get; set; }

        public MessageViewModel()
        {
        }
    }

}