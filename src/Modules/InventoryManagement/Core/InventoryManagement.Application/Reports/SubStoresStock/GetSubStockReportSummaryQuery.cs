using MediatR;

namespace InventoryManagement.Application.Reports.SubStoresStock
{
    public class GetSubStockReportSummaryQuery : IRequest<List<SubStockSummaryDto>>
    {
         public int? ItemId { get; set; }
        public int? DepartmentId { get; set; }
        public int? WarehouseId { get; set; }
        public int? StorageTypeId { get; set; }
        public int? TargetId { get; set; }
    }
}