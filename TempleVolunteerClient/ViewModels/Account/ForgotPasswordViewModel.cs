
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TempleVolunteerClient
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string? EmailAddress { get; set; }

        [Required]
        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }

        [Required]
        [Display(Name = "Temple")]
        public int TemplePropertyId { get; set; }

        public IList<SelectListItem>? TemplePropertyList { get; set; }
    }
}