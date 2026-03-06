using AutoMapper;
using BackgroundService.Domain.Common; // for BaseEntity.Status alias
using BackgroundService.Domain.Entities.Notification;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Commands.UpdateNotificationEventRule;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.DTOs;

namespace BackgroundService.Application.Notification.Common.Mappings
{
    public class NotificationHierarchyAndEventRuleProfile : Profile
    {
        public NotificationHierarchyAndEventRuleProfile()
        {
            // Enum <-> byte converters for IsActive
            CreateMap<BaseEntity.Status, byte>()
                .ConvertUsing(s => s == BaseEntity.Status.Active ? (byte)1 : (byte)0);

            CreateMap<byte, BaseEntity.Status>()
                .ConvertUsing(b => b == 1 ? BaseEntity.Status.Active : BaseEntity.Status.Inactive);

            // Entity -> DTO (GET paths)
            CreateMap<NotificationLevelHierarchy, NotificationHierarchyAndEventRuleDto>()
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive)) // uses converter
                .ForMember(d => d.NotificationEventRules, o => o.MapFrom(s => s.NotificationEventRules));

            CreateMap<NotificationEventRule, NotificationEventRuleDto>();

            // DTO -> Entity (UPDATE paths)
            CreateMap<NotificationHierarchyAndEventRuleDto, NotificationLevelHierarchy>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive)) // uses converter
                .ForMember(d => d.NotificationEventRules, o => o.Ignore());  // handle in handler

            CreateMap<NotificationEventRuleDto, NotificationEventRule>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.NotificationLevelHierarchy, o => o.Ignore())
                .ForMember(d => d.Channel, o => o.Ignore())
                .ForMember(d => d.RecipientType, o => o.Ignore())
                .ForMember(d => d.NotificationTemplates, o => o.Ignore())
                .ForMember(d => d.NotificationEventLog, o => o.Ignore());
        }
    }
}
