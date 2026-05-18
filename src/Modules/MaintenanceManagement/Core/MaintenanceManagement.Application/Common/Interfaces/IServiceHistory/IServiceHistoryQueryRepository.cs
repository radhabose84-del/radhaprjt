using MaintenanceManagement.Application.ServiceHistory.Dto;

namespace MaintenanceManagement.Application.Common.Interfaces.IServiceHistory
{
    public interface IServiceHistoryQueryRepository
    {
        Task<(List<ServiceHistoryDto> Items, int TotalCount)> GetServiceHistoryAsync(
            int? machineId,
            int? assetId,
            DateTimeOffset? fromDate,
            DateTimeOffset? toDate,
            int pageNumber,
            int pageSize);
    }
}
