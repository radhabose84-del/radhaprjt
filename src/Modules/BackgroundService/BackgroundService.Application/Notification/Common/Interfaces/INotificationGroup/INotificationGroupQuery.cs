using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Notification.NotificationGroup.Queries.GetAllNotificationGroup;
using BackgroundService.Domain.Entities.Notification;

namespace BackgroundService.Application.Notification.Common.Interfaces.INotificationGroup
{
    public interface INotificationGroupQuery
    {
        Task<(List<NotificationGroupDto>, int)> GetAllNotificationGroupAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<List<Domain.Entities.Notification.NotificationGroup>> GetNotificationGroupsAutoComplete(string searchPattern);
        Task<bool> AlreadyExistsAsync(string GroupName, int? id = null);
        Task<bool> NotFoundAsync(int id);
    }
}