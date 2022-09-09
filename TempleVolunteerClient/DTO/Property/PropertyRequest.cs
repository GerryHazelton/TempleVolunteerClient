using TempleVolunteerClient.Common;

namespace TempleVolunteerClient
{
    public class PropertyRequest : Audit
    {
        public int PropertyId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string FaxNumber { get; set; }
        public string Website { get; set; }
        public string Note { get; set; }
    }
}
