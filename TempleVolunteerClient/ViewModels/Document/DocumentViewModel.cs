
using System.ComponentModel.DataAnnotations;
using TempleVolunteerClient.Common;

namespace TempleVolunteerClient
{
    public class DocumentViewModel : Audit
    {
        public int DocumentId { get; set; }

        [Display(Name="Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Note")]
        public string? Note { get; set; }

        [Display(Name = "File Name")]
        public string DocumentFileName { get; set; }

        public string? PrevDocumentFileName { get; set; }

        [Display(Name = "Document File")]
        public IFormFile? DocumentImage { get; set; }

        public byte[]? DocumentByte { get; set; }
    }
}
