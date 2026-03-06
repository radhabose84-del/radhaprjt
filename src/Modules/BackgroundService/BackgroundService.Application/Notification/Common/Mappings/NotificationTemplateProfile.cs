using AutoMapper;
using BackgroundService.Application.Notification.NotificationTemplate.Command.CreateNotificationTemplate;
using BackgroundService.Application.Notification.NotificationTemplate.Command.DeleteNotificationTemplate;
using BackgroundService.Application.Notification.NotificationTemplate.Command.UpdateNotificationTemplate;
using BackgroundService.Application.Notification.NotificationTemplate.Queries.GetAllNotificationTemplate;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.Application.Notification.Common.Mappings
{
    public class NotificationTemplateProfile : Profile
    {
        public NotificationTemplateProfile()
        {
           CreateMap<Domain.Entities.Notification.NotificationTemplate,NotificationTemplateDto>();
           CreateMap<Domain.Entities.Notification.NotificationTemplate, NotificationTemplateAutoCompleteDto>();
            CreateMap<CreateNotificationTemplateCommand, Domain.Entities.Notification.NotificationTemplate>()
                 .ForMember(dest => dest.Id, opt => opt.Ignore())
                 .ForMember(dest => dest.NotificationTypeId, opt => opt.MapFrom(src => src.NotificationTypeId))
                 .ForMember(dest => dest.NotificationConfigId, opt => opt.MapFrom(src => src.NotificationConfigId))
                .ForMember(dest => dest.SubjectTemplate, opt => opt.MapFrom(src => src.SubjectTemplate))
                .ForMember(dest => dest.HeaderTemplate, opt => opt.MapFrom(src => src.HeaderTemplate))
                .ForMember(dest => dest.BodyTemplate, opt => opt.MapFrom(src => src.BodyTemplate))
                .ForMember(dest => dest.FooterTemplate, opt => opt.MapFrom(src => src.FooterTemplate))      
                .ForMember(dest => dest.LanguageCode, opt => opt.MapFrom(src => src.LanguageCode))      
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateNotificationTemplateCommand, Domain.Entities.Notification.NotificationTemplate>()
                .ForMember(dest => dest.NotificationTypeId, opt => opt.MapFrom(src => src.NotificationTypeId))
                .ForMember(dest => dest.NotificationConfigId, opt => opt.MapFrom(src => src.NotificationConfigId))
                .ForMember(dest => dest.SubjectTemplate, opt => opt.MapFrom(src => src.SubjectTemplate))
                .ForMember(dest => dest.HeaderTemplate, opt => opt.MapFrom(src => src.HeaderTemplate))
                .ForMember(dest => dest.BodyTemplate, opt => opt.MapFrom(src => src.BodyTemplate))
                .ForMember(dest => dest.FooterTemplate, opt => opt.MapFrom(src => src.FooterTemplate))      
                .ForMember(dest => dest.LanguageCode, opt => opt.MapFrom(src => src.LanguageCode))      
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));

              CreateMap<DeleteNotificationTemplateCommand, Domain.Entities.Notification.NotificationTemplate>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)) 
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));    
        }
    }
}