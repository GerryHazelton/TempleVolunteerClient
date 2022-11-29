
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using TempleVolunteerClient;

namespace TempleVolunteerClient
{
    [Serializable]
    public class CategoryViewModel : Audit
    {
        public int CategoryId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Note")]
        public string? Note { get; set; }
    }
}
