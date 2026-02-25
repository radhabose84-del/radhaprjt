namespace PurchaseManagement.Application.Reports.StockReport
{
    public class StockSummaryDto
    {
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public int StorageTypeId { get; set; }
        public string? StorageTypeName { get; set; }
        public int TargetId { get; set; }
        public string? TargetCode { get; set; }
        public string? TargetName { get; set; }
        public int UomId { get; set; }
        public string? UomName { get; set; }
        public decimal CurrentStockQty { get; set; }
        public decimal CurrentStockValue { get; set; }
        public decimal AvgRate { get; set; }
    }
}