
using MaintenanceManagement.Application.Common.Mappings;

namespace MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrder
{
    public class WorkOrderImageDto : IMapFrom<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder>
    {
        public string? WorkOrderImage { get; set; }
        public string? WorkOrderImageBase64 { get; set; } 
    }
}