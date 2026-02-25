using MediatR;

namespace InventoryManagement.Application.Reports.StockReportDivisionwise
{
    public class GetStockReportDivsionwiseSummaryQuery  : IRequest<List<StockSummaryDivsionwiseDto>>
    {

        public List<int>? UnitIds { get; set; } 
        public int? ItemId { get; set; }
        public int? WarehouseId { get; set; }
        public int? StorageTypeId { get; set; }
        public int? TargetId { get; set; }
    }
}