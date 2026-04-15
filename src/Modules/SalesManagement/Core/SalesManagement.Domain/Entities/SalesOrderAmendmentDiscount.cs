namespace SalesManagement.Domain.Entities
{
    public class SalesOrderAmendmentDiscount
    {
        public int Id { get; set; }
        public int SalesOrderAmendmentHeaderId { get; set; }

        // Optional — references the original SalesOrderDiscount row (null for newly added discounts)
        public int? SalesOrderDiscountId { get; set; }

        // Snapshot fields (mirror SalesOrderDiscount)
        public int DiscountMasterId { get; set; }
        public int SlabTypeId { get; set; }
        public int PaymentTermId { get; set; }
        public int? DiscountSlabId { get; set; }
        public decimal? DiscountRate { get; set; }
        public decimal? TotalDiscountValue { get; set; }

        // Navigation
        public SalesOrderAmendmentHeader? SalesOrderAmendmentHeader { get; set; }
    }
}
