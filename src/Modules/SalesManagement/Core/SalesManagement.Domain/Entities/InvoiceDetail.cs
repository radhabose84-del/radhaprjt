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
        public string? LotNo { get; set; }
        public int NoOfBags { get; set; }
        public decimal Quantity { get; set; }
        public decimal RatePerKg { get; set; }
        public decimal Discount { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal CgstPercentage { get; set; }
        public decimal SgstPercentage { get; set; }
        public decimal IgstPercentage { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal TaxAmount { get; set; }
        public int? PackTypeId { get; set; }              // FK → Sales.PackType
        public int? UOMId { get; set; }                   // Cross-module FK → Inventory.UOM
        public decimal TotalAmount { get; set; }

        // Navigation properties
        public InvoiceHeader? InvoiceHeader { get; set; }
        public PackType? PackType { get; set; }
    }
}
