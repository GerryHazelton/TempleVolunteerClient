using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using TempleVolunteerClient.Common;

namespace TempleVolunteerClient
{
    public class MyProfileViewModel : Audit
    {
        [Required]
        [MaxLength(25, ErrorMessage = "First Name cannot exceed 25 characters.")]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [MaxLength(25, ErrorMessage = "Middle Name cannot exceed 25 characters.")]
        [Display(Name = "Middle Name")]
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

        public string? EmailAddress { get; set; }

        public int? PropertyId { get; set; }

        [Display(Name = "File Name")]
        public string? StaffFileName { get; set; }

        public string? PrevStaffFileName { get; set; }

        [Display(Name = "Staff Image")]
        public IFormFile? StaffImage { get; set; }

        public byte[]? StaffByte { get; set; }

        public string? StaffByteString { get; set; }

        public int? StaffId { get; set; }

        [Display(Name = "Role")]
        public string? RoleName { get; set; }

        [Display(Name = "Remove Photo")]
        public bool RemovePhoto { get; set; }

        [Display(Name = "Event Types")]
        public ICollection<SelectListItem>? EventTypes { get; set; }

        [Display(Name = "Credentials")]
        public ICollection<SelectListItem>? Credentials { get; set; }

        public int[]? CredentialIds { get; set; }
        public int[]? RemoveCredentialIds { get; set; }
    }
}