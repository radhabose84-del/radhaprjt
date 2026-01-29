using MaintenanceManagement.Application.Common.Mappings;

namespace MaintenanceManagement.Application.WorkOrder.Command.UploadFileWorOrder.Item
{
    public class ItemImageDto : IMapFrom<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrderItem>
    {
        public string? WorkOrderItemImage { get; set; }
        public string? WorkOrderImageItemBase64 { get; set; } 
    }
}