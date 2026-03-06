using AutoMapper;
using BackgroundService.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using BackgroundService.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.Application.Mappings
{
    public class MiscTypeMasterProfile  : Profile
    {
        public MiscTypeMasterProfile()
        {
            CreateMap<Domain.Entities.Notification.MiscTypeMaster,GetMiscTypeMasterDto>();
            
            CreateMap<Domain.Entities.Notification.MiscTypeMaster, GetMiscTypeMasterAutocompleteDto>();

            CreateMap<CreateMiscTypeMasterCommand, Domain.Entities.Notification.MiscTypeMaster>()
              .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
              .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateMiscTypeMasterCommand, Domain.Entities.Notification.MiscTypeMaster>()
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));
               
            CreateMap<DeleteMiscTypeMasterCommand, Domain.Entities.Notification.MiscTypeMaster>()
               .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted)); 
        }
    }
}