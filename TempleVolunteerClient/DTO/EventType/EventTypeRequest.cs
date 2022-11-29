namespace TempleVolunteerClient
{
    public class EventTypeRequest : Audit
    {
        public int EventTypeId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Note { get; set; }
        public int[] AreaIds { get; set; }
    }
}
