using AutoMapper;
using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Commands.CreateNotificationWhatsAppGroup;
using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Commands.UpdateNotificationWhatsAppGroup;
using NotificationWhatsAppGroupEntity = BackgroundService.Core.Domain.Entities.Notifications.NotificationWhatsAppGroup;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.Application.Notification.Common.Mappings
{
    public class NotificationWhatsAppGroupProfile : Profile
    {
        public NotificationWhatsAppGroupProfile()
        {
            // Create
            CreateMap<CreateNotificationWhatsAppGroupCommand, NotificationWhatsAppGroupEntity>()
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            // Update
            CreateMap<UpdateNotificationWhatsAppGroupCommand, NotificationWhatsAppGroupEntity>()
                .ForMember(dest => dest.IsActive,
                    opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore()); 
        }
    }
}
