namespace SalesManagement.Domain.Entities
{
    public class SalesOrderDiscount
    {
        public int Id { get; set; }
        public int SalesOrderHeaderId { get; set; }
        public int DiscountMasterId { get; set; }
        public int SlabTypeId { get; set; }                  // FK → Sales.MiscMaster (SLAB_TYPE)
        public int PaymentTermId { get; set; }               // Cross-module — Purchase.PaymentTerm (no DB FK)

        // Slab snapshot — nullable because Payment_Days discounts resolve the slab at payment time
        public int? DiscountSlabId { get; set; }             // FK → Sales.DiscountSlab (optional)
        public decimal? DiscountValue { get; set; }          // Applied value snapshot (optional)

        // Same-module navigation properties
        public SalesOrderHeader? SalesOrderHeader { get; set; }
        public DiscountMaster? DiscountMaster { get; set; }
        public MiscMaster? SlabTypeMisc { get; set; }
        public DiscountSlab? DiscountSlab { get; set; }
    }
}
