using System.ComponentModel.DataAnnotations;

namespace TempleVolunteerClient
{
    public class PropertyViewModel : Audit
    {
        [Display(Name = "To")]
        public string Name { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }

        [Display(Name = "Suite, etc.")]
        public string? Address2 { get; set; }

        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "State")]
        public string? State { get; set; }

        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        [Display(Name = "Country")]
        public string Country { get; set; }

        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Fax Number")]
        public string? FaxNumber { get; set; }

        [Display(Name = "Website")]
        public string? Website { get; set; }

        [Display(Name = "Note")]
        public string? Note { get; set; }
    }
}
