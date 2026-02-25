
using MaintenanceManagement.Application.Dashboard.CardView;
using MaintenanceManagement.Application.Dashboard.Common;

namespace MaintenanceManagement.Application.Common.Interfaces.IDashboard
{
    public interface IDashboardQueryRepository
    {
        Task<ChartDto> WorkOrderSummaryAsync(DateTime fromDate, DateTime toDate, string? departmentId = null, string? machineGroupId = null);
        Task<ChartDto> ItemConsumptionSummaryAsync(DateTime fromDate, DateTime toDate, string? departmentId = null, string? machineGroupId = null);
        Task<ChartDto> ItemConsumptionDeptSummaryAsync(DateTime fromDate, DateTime toDate, string? type, string? departmentId = null, string? itemCode = null);
        Task<ChartDto> ItemConsumptionMachineSummaryAsync(DateTime fromDate, DateTime toDate, string? type, string? departmentId = null, string? itemCode = null);
        Task<ChartDto> MaintenanceHoursDeptAsync(DateTime fromDate, DateTime toDate, string? type, string? departmentId = null);
        Task<ChartDto> MaintenanceHoursMachineGroupAsync(DateTime fromDate, DateTime toDate, string? type, string? departmentId = null);
        Task<ChartDto> MaintenanceHoursMachineAsync(DateTime fromDate, DateTime toDate, string? type, string? departmentId = null, string? machineGroupId = null);
        Task<CardViewDto> GetCardDashboardAsync(DateTime fromDate, DateTime toDate,string? type,  string? departmentId = null, string? machineGroupId = null);

    }
}