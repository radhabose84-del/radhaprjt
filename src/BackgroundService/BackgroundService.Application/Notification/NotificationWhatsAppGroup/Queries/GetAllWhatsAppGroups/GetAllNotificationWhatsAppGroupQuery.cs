using System.Collections.Generic;
using BackgroundService.Application.Dto;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationWhatsAppGroup.Queries.GetAllNotificationWhatsAppGroup
{
    public class GetAllNotificationWhatsAppGroupQuery : IRequest<(List<NotificationWhatsAppGroupDto> Items, int TotalCount, int PageNumber, int PageSize)>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize  { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public int? DepartmentId { get; set; }
    }
}
