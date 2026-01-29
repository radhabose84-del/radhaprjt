using AutoMapper;
using BackgroundService.Application.MiscMaster.Command.CreateMiscMaster;
using BackgroundService.Application.MiscMaster.Command.DeleteMiscMaster;
using BackgroundService.Application.MiscMaster.Command.UpdateMiscMaster;
using BackgroundService.Application.MiscMaster.Queries.GetMiscMaster;
using static BackgroundService.Domain.Common.BaseEntity;


namespace BackgroundService.Application.Mappings
{
    public class MiscMasterProfile  : Profile
    {
        public MiscMasterProfile()
        {
            CreateMap<Domain.Entities.Notification.MiscMaster, GetMiscMasterDto>();

            CreateMap<Domain.Entities.Notification.MiscMaster, GetMiscMasterAutoCompleteDto>();

            CreateMap<CreateMiscMasterCommand, Domain.Entities.Notification.MiscMaster>()
           .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
           .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateMiscMasterCommand, Domain.Entities.Notification.MiscMaster>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<DeleteMiscMasterCommand, Domain.Entities.Notification.MiscMaster>()
           .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));
        }
    }
}