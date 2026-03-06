using System.Collections.Generic;
using BackgroundService.Application.Dto;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationWhatsAppGroup.Queries.GetNotificationWhatsAppGroupAutoComplete
{
    public class GetNotificationWhatsAppGroupAutoCompleteQuery : IRequest<List<NotificationWhatsAppGroupAutoCompleteDto>>
    {
        public string? SearchTerm { get; set; }
    }
}
