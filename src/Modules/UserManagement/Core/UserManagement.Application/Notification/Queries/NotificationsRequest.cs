using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using MediatR;

namespace UserManagement.Application.Notification.Queries
{
    public class NotificationRequest : IRequest<NotificationResponse>
    {
        public string? Username { get; set; }
    }
}