
namespace MaintenanceManagement.Application.Common.Interfaces.IWorkOrder
{
    public interface IWorkOrderCommandRepository
    {

        public Task<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder> CreateAsync(MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder workOrder, int requestTypeId, CancellationToken cancellationToken);
        public Task<bool> UpdateAsync(int workOrderId, MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder workOrder);
        Task<int> CreateScheduleAsync(int workOrderId, MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrderSchedule workOrderSchedule);
        Task<bool> UpdateScheduleAsync(int workOrderId, MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrderSchedule workOrderSchedule);
        Task<bool> UpdateWOImageAsync(int workOrderId, string imageName);
        Task<bool> DeleteWOImageAsync(string imageName);
        Task<bool> DeleteItemImageAsync(string imageName);
        Task<bool> UpdateWOItemImageAsync(int workOrderId, string imageName);
        Task<bool> RemoveWOImageReferenceAsync(int workOrderId);
        Task<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder> GetByIdAsync(int workOrderId);
        Task<string?> GetLatestWorkOrderDocNo(int TypeId);
        Task<string> GetBaseDirectoryItemAsync();
        Task<MaintenanceManagement.Domain.Entities.MiscMaster> GetMiscMasterByCodeAsync(string code);
        Task<bool> RevertWorkOrderStatusAsync(int workOrderId);
        public Task<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder> CreatePreventiveAsync(MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder workOrder, int requestTypeId, int companyId, int unitId, CancellationToken cancellationToken);
        
        public Task<bool> UpdateRequestDateAsync(
            int workOrderId,
            DateTimeOffset requestDate,int isSystemTime,
            CancellationToken cancellationToken = default);

    }
}