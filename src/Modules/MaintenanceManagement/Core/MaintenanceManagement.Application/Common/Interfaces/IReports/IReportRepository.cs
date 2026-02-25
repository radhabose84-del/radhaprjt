
using MaintenanceManagement.Application.Reports.GetStockLegerReport;
using MaintenanceManagement.Application.Reports.MaintenanceRequestReport;
using MaintenanceManagement.Application.Reports.WorkOrderItemConsuption;
using MaintenanceManagement.Application.Reports.WorkOrderReport;
using MaintenanceManagement.Application.StockLedger.Queries.GetCurrentStock;
using MaintenanceManagement.Application.Reports.WorkOderCheckListReport;
using MaintenanceManagement.Application.Reports.MRS;
using MaintenanceManagement.Application.Reports.PowerConsumption;
using MaintenanceManagement.Application.Reports.GeneratorConsumption;

namespace MaintenanceManagement.Application.Common.Interfaces.IReports
{
    public interface IReportRepository
    {

        Task<List<RequestReportDto>> MaintenanceReportAsync(DateTimeOffset? requestFromDate, DateTimeOffset? requestToDate, int? RequestType, int? requestType, int? departmentId);
        Task<List<WorkOrderReportDto>> WorkOrderReportAsync(DateTimeOffset? fromDate, DateTimeOffset? toDate, int? RequestTypeId, int? departmentId);
        Task<List<WorkOrderIssueDto>> GetItemConsumptionAsync(DateTimeOffset IssueFromDate, DateTimeOffset IssueToDate);
        Task<List<StockLedgerReportDto>> GetSubStoresStockLedger(string OldUnitcode, DateTime FromDate, DateTime ToDate, string? Itemcode, int departmentId);
        Task<List<CurrentStockDto>> GetStockDetails(string OldUnitcode, int departmentId);
        Task<List<WorkOderCheckListReportDto>> GetWorkOrderChecklistReportAsync(DateTimeOffset? requestFromDate, DateTimeOffset? requestToDate, int? machineGroupId, int? machineId, int? activityId);
        Task<List<MRSReportDto>> GetMRSReports(DateTimeOffset IssueFromDate, DateTimeOffset IssueToDate, string OldUnitCode);
        Task<IEnumerable<dynamic>> ScheduleReportAsync(DateTime? FromDueDate, DateTime? ToDueDate);
        Task<IEnumerable<dynamic>> MaterialPlanningReportAsync(DateTime? FromDueDate, DateTime? ToDueDate);
        Task<List<PowerReportDto>> GetPowerReports(DateTimeOffset? FromDate, DateTimeOffset? ToDate);
        Task<List<GeneratorReportDto>> GetGeneratorReports(DateTimeOffset? FromDate, DateTimeOffset? ToDate);
    }
}