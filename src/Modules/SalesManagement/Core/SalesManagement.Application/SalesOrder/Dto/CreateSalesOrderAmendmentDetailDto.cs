namespace SalesManagement.Application.SalesOrder.Dto
{
    public class CreateSalesOrderAmendmentDetailDto
    {
        // References the existing SalesOrderDetail row being changed
        public int SalesOrderDetailId { get; set; }

        // New Values — provide at least one for "Modified"; leave all null for "Removed"
        // ChangeType is auto-derived: any New* value set → "Modified", all null → "Removed"
        public int? NewQtyInBags { get; set; }
        public decimal? NewExMillRate { get; set; }
        public DateOnly? NewExpectedDeliveryDate { get; set; }

        // Detail-level Computed Fields
        public decimal TotalWeight { get; set; }
        public decimal DiscountPerUnit { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TCSAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal NetRatePerKg { get; set; }
        public int PendingQty { get; set; }
        public decimal? AgentCommissionPercentage { get; set; }
    }
}
