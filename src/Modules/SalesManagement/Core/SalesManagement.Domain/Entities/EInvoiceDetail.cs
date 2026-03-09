namespace SalesManagement.Domain.Entities
{
    // Plain class — no BaseEntity; detail records have no independent audit trail
    public class EInvoiceDetail
    {
        public int Id { get; set; }
        public int EInvoiceHeaderId { get; set; }         // FK → Sales.EInvoiceHeader
        public int ItemSno { get; set; }
        public int ItemId { get; set; }                   // Cross-module FK → InventoryManagement
        public string? ItemName { get; set; }
        public string? HsnNo { get; set; }
        public int NoOfBags { get; set; }
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Rate { get; set; }
        public decimal Discount { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal GstPercentage { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal TotalAmount { get; set; }
        public int? PackTypeId { get; set; }              // FK → Sales.PackType
        public string? UOM { get; set; }

        // Navigation properties
        public EInvoiceHeader? EInvoiceHeader { get; set; }
    }
}
