using System.ComponentModel.DataAnnotations;

namespace TempleVolunteerClient
{
    public class ResetPasswordViewModel
    {
        public string? EmailAddress { get; set; }
        public string? Token { get; set; }

        [Required]
        [StringLength(30, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Old Password")]
        public string? OldPassword { get; set; }

        [Required]
        [StringLength(30, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New Password Confirm")]
        [Compare("Password", ErrorMessage = "The new password and new confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Property")]
        public int PropertyId { get; set; }
    }
}