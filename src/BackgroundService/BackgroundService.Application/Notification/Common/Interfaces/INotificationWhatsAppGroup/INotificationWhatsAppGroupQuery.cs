using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackgroundService.Application.Dto;

namespace BackgroundService.Application.Notification.Common.Interfaces.INotificationWhatsAppGroup
{
    public interface INotificationWhatsAppGroupQuery
    {
        Task<(List<NotificationWhatsAppGroupDto> Items, int TotalCount)> GetAllAsync(
            NotificationWhatsAppGroupListFilterDto filter,
            CancellationToken ct = default);

        Task<NotificationWhatsAppGroupDto?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<List<NotificationWhatsAppGroupAutoCompleteDto>> GetAutoCompleteAsync(string? searchTerm, CancellationToken ct = default);

        Task<List<NotificationWhatsAppGroupAutoCompleteDto>> GetByDepartmentAsync(int departmentId, string? searchTerm, CancellationToken ct = default);
    }
}
