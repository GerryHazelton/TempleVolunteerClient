namespace TempleVolunteerClient.Common
{
    public class AppSettings
    {
        public string? UriLocal { get; set; }
        public string? UriHiranyaloka { get; set; }
        public string? UriProduction { get; set; }
        public string? ContentType { get; set; }
        public string? TempPassword { get; set; }

        public AppSettings ()
        {
            if (UriLocal == null) UriLocal = "https://localhost:7289";
            if (UriHiranyaloka == null) UriHiranyaloka = "http://hiranyaloka:8181";
            if (UriProduction == null) UriProduction = "http://75.80.152.255:8181";
            if (ContentType == null) ContentType = "application/json";
            if (TempPassword == null) TempPassword = "Master1952SRF!";
        }
    }
}
