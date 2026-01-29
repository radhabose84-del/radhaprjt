
using MaintenanceManagement.Domain.Common;

namespace MaintenanceManagement.Domain.Entities.WorkOrderMaster
{
    public class WorkOrderItem
    {
        public int Id { get; set; }
        public int? WorkOrderId { get; set; }
        public WorkOrder WOItem { get; set; } = null!;
        public int? StoreTypeId { get; set; }
        public MiscMaster MiscStoreType { get; set; } = null!;
        public string? ItemCode { get; set; }
        public string? OldItemCode { get; set; }
        public string? ItemName { get; set; }
        public int AvailableQty { get; set; }
        public int UsedQty { get; set; }
        public int? ScarpQty { get; set; }
        public int? ToSubStoreQty { get; set; }
        public string? Image { get; set; }
        public decimal? Rate { get; set; }
        public int? DepartmentId { get; set; }
    }
}
