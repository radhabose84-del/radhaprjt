using MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrder;

namespace MaintenanceManagement.Application.Common.Interfaces.IWorkOrder
{
    public interface IWorkOrderQueryRepository
    {
        Task<List<MaintenanceManagement.Domain.Entities.MiscMaster>> GetWORootCauseDescAsync();
        Task<List<MaintenanceManagement.Domain.Entities.MiscMaster>> GetWOStatusDescAsync();
        Task<List<MaintenanceManagement.Domain.Entities.MiscMaster>> GetWOSourceDescAsync();
        Task<List<MaintenanceManagement.Domain.Entities.MiscMaster>> GetWOStoreTypeDescAsync();
        Task<List<MaintenanceManagement.Domain.Entities.MiscMaster>> GetRequestTypeAsync();
        Task<List<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder>> GetWorkOrderAsync();
        Task<string> GetBaseDirectoryAsync();
        Task<(dynamic WorkOrderResult, IEnumerable<dynamic> Activity, IEnumerable<dynamic> Item, IEnumerable<dynamic> Technician, IEnumerable<dynamic> checkList, IEnumerable<dynamic> schedule)> GetWorkOrderByIdAsync(int workOrderId);
        Task<List<WorkOrderWithScheduleDto>> GetAllWOAsync(DateTimeOffset? fromDate, DateTimeOffset? toDate, int? requestType, int? departmentId,int? machineId);   
        Task<bool> ValidateRequestDateAsync(int workOrderId, DateTimeOffset requestDate, int isSystemTime, CancellationToken cancellationToken = default);                 
    }
}
