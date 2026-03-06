using System.Collections.Generic;
using BackgroundService.Application.Dto;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationWhatsAppGroup.Queries.GetNotificationWhatsAppGroupByDepartment
{
    public class GetNotificationWhatsAppGroupByDepartmentQuery : IRequest<List<NotificationWhatsAppGroupAutoCompleteDto>>
    {
        public int DepartmentId { get; set; }
        public string? SearchTerm { get; set; }
    }
}
