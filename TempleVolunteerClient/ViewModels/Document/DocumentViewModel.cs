
using System.ComponentModel.DataAnnotations;
using TempleVolunteerClient.Common;

namespace TempleVolunteerClient
{
    public class DocumentViewModel : Audit
    {
        public DocumentViewModel()
        {

        }

        public int DocumentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
        public string DocumentFileName { get; set; }
        public byte[]? DocumentImage { get; set; }
    }
}