using AutoMapper;

namespace TempleVolunteerClient
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AreaViewModel, AreaRequest>();
            CreateMap<AreaRequest, AreaViewModel>();

            CreateMap<MyProfileViewModel, MyProfileRequest>();
            CreateMap<MyProfileRequest, MyProfileViewModel>();

            CreateMap<MyProfileViewModel, StaffRequest>();
            CreateMap<StaffRequest, MyProfileViewModel>();
        }
    }
}
