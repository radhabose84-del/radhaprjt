using AutoMapper;
using BackgroundService.Application.Notification.GetNotificationDetail.UpdateNotificationStatus;
using BackgroundService.Domain.Entities.Notification;

namespace BackgroundService.Application.Notification.Common.Mappings
{
    public class NotificationLogProfile : Profile
    {
        public NotificationLogProfile()
        {
             CreateMap<UpdateNotificationStatus, NotificationEventLog>();
        }
    }
}