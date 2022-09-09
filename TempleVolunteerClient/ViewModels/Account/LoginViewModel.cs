using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TempleVolunteerClient
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email Address")]
        [EmailAddress]
        public string? EmailAddress { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Required]
        [Display(Name = "Temple")]
        public int TemplePropertyId { get; set; }

        public IList<SelectListItem>? TemplePropertyList { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        public int PropertyId { get; set; }
    }
}