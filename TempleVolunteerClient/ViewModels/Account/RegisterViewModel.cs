using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using TempleVolunteerClient.Common;

namespace TempleVolunteerClient
{
    public class RegisterViewModel
    {
        [Required]
        [MaxLength(25, ErrorMessage = "First Name cannot exceed 25 characters.")]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [MaxLength(25, ErrorMessage = "Middle Name cannot exceed 25 characters.")]
        [Display(Name = "Last Name")]
        public string? MiddleName { get; set; }

        [Required]
        [MaxLength(25, ErrorMessage = "Last Name cannot exceed 25 characters.")]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Required]
        [MaxLength(100, ErrorMessage = "Address cannot exceed 100 characters.")]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [MaxLength(10, ErrorMessage = "Address 2 cannot exceed 10 characters.")]
        [Display(Name = "Address 2")]
        public string? Address2 { get; set; }

        [Required]
        [MaxLength(40, ErrorMessage = "City cannot exceed 40 characters.")]
        [Display(Name = "City")]
        public string? City { get; set; }

        [MaxLength(2, ErrorMessage = "State cannot exceed 2 characters.")]
        [Display(Name = "State")]
        public string? State { get; set; }

        public IList<SelectListItem> States = ListHelpers.States;

        [Required]
        [MaxLength(15, ErrorMessage = "Postal code cannot exceed 15 characters.")]
        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }

        [Required]
        [MaxLength(25, ErrorMessage = "Country cannot exceed 25 characters.")]
        [Display(Name = "Country")]
        public string? Country { get; set; }

        public IList<SelectListItem> Countries = ListHelpers.Countries;

        [Required]
        [MaxLength(50, ErrorMessage = "Email Address cannot exceed 50 characters.")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        public string? EmailAddress { get; set; }

        [Required]
        [StringLength(30, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        [Required]
        [Phone]
        [MaxLength(15, ErrorMessage = "Phone number cannot exceed 15 characters.")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Gender")]
        public string? Gender { get; set; }

        public IList<SelectListItem>? GenderList { get; set; }

        [Required]
        [Display(Name = "First Aid")]
        public bool FirstAid { get; set; }

        [Required]
        [Display(Name = "CPR")]
        public bool CPR { get; set; }

        [Display(Name = "Kriyaban")]
        public bool Kriyaban { get; set; }

        [Display(Name = "Lesson Student")]
        public bool LessonStudent { get; set; }

        [Required]
        [Display(Name = "Accept Terms")]
        public bool AcceptTerms { get; set; }

        [Required]
        [Display(Name = "Temple")]
        public int TemplePropertyId { get; set; }

        public IList<SelectListItem>? TemplePropertyList { get; set; }

        public bool? CanSendMessages { get; set; }
        public bool? CanViewDocuments { get; set; }
        public bool? EmailConfirmed { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? VerifiedDate { get; set; }
    }
}