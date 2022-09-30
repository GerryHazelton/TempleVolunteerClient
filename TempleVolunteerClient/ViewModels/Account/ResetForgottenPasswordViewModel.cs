using AutoMapper.Configuration.Annotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TempleVolunteerClient
{
    public class ResetForgottenPasswordViewModel
    {
        [Required]
        [StringLength(8, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm")]
        [Compare("Password", ErrorMessage = "Confirmation password does not match.")]
        public string? ConfirmPassword { get; set; }

        [Ignore]
        public string TempleName { get; set; }
        [Ignore]
        public string EmailAddress { get; set; }
        [Ignore]
        public int PropertyId { get; set; }
    }
}