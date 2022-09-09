
using System.ComponentModel.DataAnnotations;
using TempleVolunteerClient.Common;

namespace TempleVolunteerClient
{
    public class AreaViewModel : Audit
    {
        public int AreaId { get; set; }

        [Required]
        [Display(Name = "Name")]
        [StringLength(50, ErrorMessage = "Name cannot be longer than 50 characters.")]
        public string? Name { get; set; }

        [Required]
        [Display(Name = "Description")]
        [StringLength(250, ErrorMessage = "Description cannot be longer than 250 characters.")]
        public string? Description { get; set; }

        [Display(Name = "Area Number")]
        [StringLength(10, ErrorMessage = "Area number cannot be longer than 10 characters.")]
        public string? AreaNumber { get; set; }

        [Display(Name = "Note")]
        [StringLength(250, ErrorMessage = "Note cannot be longer than 250 characters.")]
        public string? Note { get; set; }

        public AreaViewModel()
        {
        }
    }
}