namespace Contracts.Dtos.Stock
{
    public class SalesStockLedgerDto
    {
        public int UnitId { get; set; }
        public string? DocType { get; set; }
        public int DocNo { get; set; }
        public int DetailDocNo { get; set; }
        public DateTime DocDate { get; set; }
        public int ItemId { get; set; }
        public int LotId { get; set; }
        public int PackNo { get; set; }
        public int PackTypeId { get; set; }
        public int WarehouseId { get; set; }
        public int BinId { get; set; }
        public int TotalQty { get; set; }
        public decimal TotalValue { get; set; }
        public int StatusId { get; set; }
    }
}
