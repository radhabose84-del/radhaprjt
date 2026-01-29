using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.Notification.Queries
{
    public class NotificationRequest : IRequest<NotificationResponse>
    {
        public string? Username { get; set; }
    }
}