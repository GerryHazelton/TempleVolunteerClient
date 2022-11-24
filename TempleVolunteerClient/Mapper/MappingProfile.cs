using AutoMapper;

namespace TempleVolunteerClient
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AreaViewModel, AreaRequest>();
            CreateMap<AreaRequest, AreaViewModel>();

            CreateMap<CategoryViewModel, CategoryRequest>();
            CreateMap<CategoryRequest, CategoryViewModel>();

            CreateMap<CommitteeViewModel, CommitteeRequest>();
            CreateMap<CommitteeRequest, CommitteeViewModel>();

            CreateMap<CredentialViewModel, CredentialRequest>();
            CreateMap<CredentialRequest, CredentialViewModel>();

            CreateMap<DocumentViewModel, DocumentRequest>();
            CreateMap<DocumentRequest, DocumentViewModel>();

            CreateMap<EventViewModel, EventRequest>();
            CreateMap<EventRequest, EventViewModel>();

            CreateMap<EventTaskViewModel, EventTaskRequest>();
            CreateMap<EventTaskRequest, EventTaskViewModel>();

            CreateMap<EventTypeViewModel, EventTypeRequest>();
            CreateMap<EventTypeRequest, EventTypeViewModel>();

            CreateMap<MessageViewModel, MessageRequest>();
            CreateMap<MessageRequest, MessageViewModel>();

            CreateMap<MyProfileViewModel, MyProfileRequest>();
            CreateMap<MyProfileRequest, MyProfileViewModel>();

            CreateMap<MyProfileViewModel, StaffRequest>();
            CreateMap<StaffRequest, MyProfileViewModel>();

            CreateMap<PropertyViewModel, PropertyRequest>();
            CreateMap<PropertyRequest, PropertyViewModel>();

            CreateMap<RoleViewModel, RoleRequest>();
            CreateMap<RoleRequest, RoleViewModel>();

            CreateMap<SupplyItemViewModel, SupplyItemRequest>();
            CreateMap<SupplyItemRequest, SupplyItemViewModel>();
        }
    }
}
