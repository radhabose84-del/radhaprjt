namespace SalesManagement.Domain.Entities
{
    public class StockLedger
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public string? DocType { get; set; }
        public int DocNo { get; set; }
        public int DocSno { get; set; }
        public DateOnly DocDate { get; set; }
        public int ItemId { get; set; }
        public int PackNo { get; set; }
        public int PackTypeId { get; set; }
        public int WarehouseId { get; set; }
        public int BinId { get; set; }
        public int TotalQty { get; set; }
        public decimal TotalValue { get; set; }
    }
}
