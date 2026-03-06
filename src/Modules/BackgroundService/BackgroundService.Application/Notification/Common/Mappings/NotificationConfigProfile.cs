using AutoMapper;
using BackgroundService.Application.Notification.NotificationConfig.Command.CreateNotificationConfig;
using BackgroundService.Application.Notification.NotificationConfig.Command.DeleteNotificationConfig;
using BackgroundService.Application.Notification.NotificationConfig.Command.UpdateNotificationConfig;
using BackgroundService.Application.Notification.NotificationConfig.Queries.GetAllNotificationConfig;
using BackgroundService.Application.Notification.NotificationConfig.Queries.GetNotificationConfigAutoComplete;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.Application.Notification.Common.Mappings
{
    public class NotificationConfigProfile : Profile
    {
        public NotificationConfigProfile()
        {
           CreateMap<Domain.Entities.Notification.NotificationConfig,NotificationConfigDto>();
           CreateMap<Domain.Entities.Notification.NotificationConfig, NotificationConfigAutoCompleteDto>(); 
           CreateMap<CreateNotificationConfigCommand, Domain.Entities.Notification.NotificationConfig>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ModuleName, opt => opt.MapFrom(src => src.ModuleName))
                .ForMember(dest => dest.NotificationEventTypeId, opt => opt.MapFrom(src => src.NotificationEventTypeId))                
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));


            CreateMap<UpdateNotificationConfigCommand, Domain.Entities.Notification.NotificationConfig>()
               .ForMember(dest => dest.ModuleName, opt => opt.MapFrom(src => src.ModuleName))
                .ForMember(dest => dest.NotificationEventTypeId, opt => opt.MapFrom(src => src.NotificationEventTypeId))     
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));


              CreateMap<DeleteNotificationConfigCommand, Domain.Entities.Notification.NotificationConfig>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)) 
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));    
        }
    }
}