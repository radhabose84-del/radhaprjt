namespace MaintenanceManagement.Domain.Entities
{
    public class ItemTransactions
    {
        public int Id { get; set; }
        public string? OldUnitCode { get; set; }
        public int TC { get; set; }
        public string? TransactionType { get; set; } // "Issue" or "Return"
        public int DocNo { get; set; }
        public int DocSNo { get; set; }
        public DateTime DocDate { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? UOM { get; set; }
        public decimal Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal Value { get; set; }
        public string? CategoryDescription { get; set; }
        public string? GroupName { get; set; }
        public string? LifeType { get; set; }
        public int LifeSpan { get; set; }
        public string? DepartmentName { get; set; }
        public DateTime CreatedDate { get; set; }
       
    }
}