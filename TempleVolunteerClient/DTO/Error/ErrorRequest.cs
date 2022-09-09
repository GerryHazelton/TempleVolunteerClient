using TempleVolunteerClient.Common;

namespace TempleVolunteerClient
{
    public class ErrorRequest : Audit
    {
        public string FunctionName { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
        public string Environment { get; set; }
        public int PropertyId { get; set; }
    }
}
