namespace FinanceManagement.Domain.Entities
{
    // Plain class — no BaseEntity; detail records have no independent audit trail
    public class EWaybillDetail
    {
        public int Id { get; set; }
        public int EWaybillHeaderId { get; set; }         // FK → Finance.EWaybillHeader (Cascade Delete)
        public int ItemSno { get; set; }
        public int ItemId { get; set; }                   // Cross-module FK → InventoryManagement
        public string? ItemName { get; set; }
        public string? HsnNo { get; set; }
        public string? IsService { get; set; }            // Y = Service, N = Goods
        public decimal Qty { get; set; }
        public string? UOM { get; set; }
        public decimal TaxableValue { get; set; }
        public decimal TaxRate { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal Cess { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        // Navigation property
        public EWaybillHeader? EWaybillHeader { get; set; }
    }
}
