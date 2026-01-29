using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationGroup.Queries.GetNotificationGroupAutoComplete
{
    public class GetNotificationGroupAutoCompleteQuery : IRequest<List<GetNotificationGroupAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; } 
    }
}