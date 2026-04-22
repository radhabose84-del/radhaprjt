namespace SalesManagement.Domain.Entities
{
    // Plain class — no BaseEntity; detail records have no independent audit trail or soft-delete
    public class InvoiceDetail
    {
        public int Id { get; set; }
        public int InvoiceHeaderId { get; set; }          // FK → Sales.InvoiceHeader
        public int ItemSno { get; set; }
        public int ItemId { get; set; }                   // Cross-module FK → InventoryManagement
        public string? HsnCode { get; set; }
        public decimal GstPercentage { get; set; }
        public int? LotId { get; set; }                   // Cross-module FK → ProductionManagement
        public int NoOfBags { get; set; }
        public decimal Quantity { get; set; }
        public decimal RatePerKg { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal FreightValue { get; set; }
        public decimal CommissionValue { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal CgstPercentage { get; set; }
        public decimal SgstPercentage { get; set; }
        public decimal IgstPercentage { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal TaxAmount { get; set; }
        public int? PackTypeId { get; set; }              // Cross-module FK → ProductionManagement
        public int? UOMId { get; set; }                   // Cross-module FK → Inventory.UOM
        public decimal Charity { get; set; }
        public decimal HandlingCharges { get; set; }
        public decimal TotalAmount { get; set; }

        // Navigation properties
        public InvoiceHeader? InvoiceHeader { get; set; }
        // PackType and LotMaster moved to ProductionManagement — use lookups for names
    }
}
