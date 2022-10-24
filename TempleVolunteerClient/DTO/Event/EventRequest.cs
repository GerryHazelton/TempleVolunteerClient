namespace TempleVolunteerClient
{
    public class EventRequest : Audit
    {
        public int EventId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Note { get; set; }
        public int[] EventTypeIds { get; set; }
    }
}
