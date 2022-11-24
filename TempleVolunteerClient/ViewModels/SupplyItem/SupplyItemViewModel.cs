
using System.ComponentModel.DataAnnotations;
using TempleVolunteerClient.Common;

namespace TempleVolunteerClient
{
    public class SupplyItemViewModel : Audit
    {
        public int SupplyItemId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Note")]
        public string Note { get; set; }

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Bin Number")]
        public string BinNumber { get; set; }

        [Display(Name = "File Name")]
        public string SupplyItemFileName { get; set; }

        [Display(Name = "Document File")]
        public IFormFile SupplyItemImage { get; set; }

        public byte[]? SupplyItemByte { get; set; }

        public byte[]? PrevSupplyItemImage { get; set; }
    }
}