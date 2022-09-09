
using System.Text.Json.Serialization;

namespace TempleVolunteerClient
{
    public abstract class BaseResponse
    {
        [JsonIgnore()]
        public bool Success { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ErrorCode { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Message { get; set; }
    }
}
