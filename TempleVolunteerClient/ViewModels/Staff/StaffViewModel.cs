using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using TempleVolunteerClient.Common;

namespace TempleVolunteerClient
{
    public class StaffViewModel : Audit
    {
        public int StaffId { get; set; } 
        
        [Required]
        [MaxLength(25, ErrorMessage = "First Name cannot exceed 25 characters.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        public int LoggedInStaff { get; set; }

        [MaxLength(25, ErrorMessage = "Middle Name cannot exceed 25 characters.")]
        [Display(Name = "Middle Name")]
        public string? MiddleName { get; set; }

        [Required]
        [MaxLength(25, ErrorMessage = "Last Name cannot exceed 25 characters.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

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

        [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Can Order Supplies")]
        public bool CanOrderSupplies { get; set; }

        [Display(Name = "Can View Reports")]
        public bool CanViewReports { get; set; }

        [Display(Name = "Can Send Messages")]
        public bool CanSendMessages { get; set; }

        [Required]
        [Display(Name = "Role")]
        public int RoleId { get; set; }

        public IList<SelectListItem> RoleList;

        [Required]
        [MaxLength(15, ErrorMessage = "Phone number cannot exceed 15 characters.")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Gender")]
        public string? Gender { get; set; }

        public IList<SelectListItem> GenderList { get; set; }

        [Display(Name = "Trained in First Aid")]
        public bool FirstAid { get; set; }

        [Display(Name = "Trained in CPR")]
        public bool CPR { get; set; }

        [Display(Name = "Kriyaban")]
        public bool Kriyaban { get; set; }

        [Display(Name = "Lesson Student")]
        public bool LessonStudent { get; set; }

        [Display(Name = "Is Verified")]
        public bool IsVerified { get; set; }

        [Display(Name = "Verified Date")]
        [DataType(DataType.Date)]
        public DateTime VerifiedDate { get; set; }

        [Display(Name = "Unlock User")]
        public bool UnlockUser { get; set; }

        public bool IsLockedOut { get; set; }

        [Display(Name = "Number of tries logging-in")]
        public string? LoginAttempts { get; set; }

        public string? PasswordHash { get; set; }
        public bool IsAdmin { get; set; }

        [Display(Name = "File Name")]
        public string StaffFileName { get; set; }

        [Display(Name = "Staff File")]
        public IFormFile StaffImage { get; set; }

        public byte[]? StaffByte { get; set; }

        public byte[]? PrevStaffImage { get; set; }
    }
}