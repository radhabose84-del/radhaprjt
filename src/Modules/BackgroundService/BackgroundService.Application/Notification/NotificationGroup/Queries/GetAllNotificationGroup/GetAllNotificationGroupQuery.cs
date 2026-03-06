using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationGroup.Queries.GetAllNotificationGroup
{
    public class GetAllNotificationGroupQuery : IRequest<ApiResponseDTO<List<NotificationGroupDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}