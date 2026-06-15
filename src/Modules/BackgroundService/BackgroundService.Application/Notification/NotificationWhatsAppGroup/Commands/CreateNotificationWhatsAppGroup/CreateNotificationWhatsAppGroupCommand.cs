using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Contracts.Common;

namespace BackgroundService.Application.Notification.NotificationWhatsAppGroup.Commands.CreateNotificationWhatsAppGroup
{
    public class CreateNotificationWhatsAppGroupCommand : IRequest<int>, IRequirePermission
    {
        public int DepartmentId { get; set; }

        public string GroupName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
                
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
} 
