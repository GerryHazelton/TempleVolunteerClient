namespace TempleVolunteerClient
{
    public class MyProfileResponse : BaseResponse
    {
        public MyProfileRequest Staff { get; set; }
        public Exception Error { get; set; }
    }
}
