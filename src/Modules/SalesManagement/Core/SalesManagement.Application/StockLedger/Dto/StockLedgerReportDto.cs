namespace SalesManagement.Application.StockLedger.Dto
{
    public class StockLedgerReportDto
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public int? SourceUnitId { get; set; }
        public string? SourceUnitName { get; set; }
        public string? DocType { get; set; }
        public int DocNo { get; set; }
        public int DetailDocNo { get; set; }
        public DateOnly DocDate { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public int LotId { get; set; }
        public string? LotCode { get; set; }
        public int PackNo { get; set; }
        public int PackTypeId { get; set; }
        public string? PackTypeName { get; set; }
        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public int BinId { get; set; }
        public string? BinName { get; set; }
        public int TotalQty { get; set; }
        public decimal TotalValue { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
    }
}
