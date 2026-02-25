namespace MaintenanceManagement.Domain.Entities
{
    public class StockLedger
    {
        public int Id { get; set; }
        public string? OldUnitCode { get; set; }
        public string? TransactionType { get; set; } // "Receipt" or "Return or issue"
        public int DocNo { get; set; }
        public int DocSNo { get; set; }
        public DateTime DocDate { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? UOM { get; set; }
        public decimal ReceivedQty { get; set; }
        public decimal ReceivedValue { get; set; }
        public decimal IssueQty { get; set; }
        public decimal IssueValue { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}