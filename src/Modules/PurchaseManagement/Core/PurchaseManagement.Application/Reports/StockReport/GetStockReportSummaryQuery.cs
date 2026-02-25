using MediatR;

namespace PurchaseManagement.Application.Reports.StockReport
{
    public class GetStockReportSummaryQuery : IRequest<List<StockSummaryDto>>
    {
        public int? ItemId { get; set; }
        public int? WarehouseId { get; set; }
        public int? StorageTypeId { get; set; }
        public int? TargetId { get; set; }
    }
}