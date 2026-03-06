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
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.MiscType, opt => opt.Ignore())
                .ForMember(dest => dest.TargetType, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovalMode, opt => opt.Ignore())
                .ForMember(dest => dest.Channels, opt => opt.Ignore())
                .ForMember(dest => dest.RecipientType, opt => opt.Ignore())
                .ForMember(dest => dest.NotificationEventType, opt => opt.Ignore())
                .ForMember(dest => dest.Channel, opt => opt.Ignore())
                .ForMember(dest => dest.NotificationStatus, opt => opt.Ignore())
                .ForMember(dest => dest.NotificationTemplates, opt => opt.Ignore())
                .ForMember(dest => dest.ReadStatus, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovalStep, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovalType, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovalRequestStatus, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovalRequestLineStatus, opt => opt.Ignore())
                .ForMember(dest => dest.Action, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovalTargetType, opt => opt.Ignore())
                .ForMember(dest => dest.Operator, opt => opt.Ignore())
                .ForMember(dest => dest.RightType, opt => opt.Ignore())
                .ForMember(dest => dest.ValueType, opt => opt.Ignore())
                .ForMember(dest => dest.Scope, opt => opt.Ignore());

            CreateMap<UpdateMiscMasterCommand, Domain.Entities.Notification.MiscMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(dest => dest.MiscType, opt => opt.Ignore())
                .ForMember(dest => dest.TargetType, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovalMode, opt => opt.Ignore())
                .ForMember(dest => dest.Channels, opt => opt.Ignore())
                .ForMember(dest => dest.RecipientType, opt => opt.Ignore())
                .ForMember(dest => dest.NotificationEventType, opt => opt.Ignore())
                .ForMember(dest => dest.Channel, opt => opt.Ignore())
                .ForMember(dest => dest.NotificationStatus, opt => opt.Ignore())
                .ForMember(dest => dest.NotificationTemplates, opt => opt.Ignore())
                .ForMember(dest => dest.ReadStatus, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovalStep, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovalType, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovalRequestStatus, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovalRequestLineStatus, opt => opt.Ignore())
                .ForMember(dest => dest.Action, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovalTargetType, opt => opt.Ignore())
                .ForMember(dest => dest.Operator, opt => opt.Ignore())
                .ForMember(dest => dest.RightType, opt => opt.Ignore())
                .ForMember(dest => dest.ValueType, opt => opt.Ignore())
                .ForMember(dest => dest.Scope, opt => opt.Ignore());

            CreateMap<DeleteMiscMasterCommand, Domain.Entities.Notification.MiscMaster>()
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted))
                .ForMember(dest => dest.MiscType, opt => opt.Ignore());
        }
    }
}