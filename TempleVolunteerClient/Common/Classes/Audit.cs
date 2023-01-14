namespace TempleVolunteerClient
{
    public class Audit
    {
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsActive { get; set;}
        public bool IsHidden { get; set; }
        public int PropertyId { get; set; }
        public bool IsAdmin { get; set; }
    }
}
