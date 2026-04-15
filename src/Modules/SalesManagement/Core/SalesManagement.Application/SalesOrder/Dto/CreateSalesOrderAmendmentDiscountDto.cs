namespace SalesManagement.Application.SalesOrder.Dto
{
    public class CreateSalesOrderAmendmentDiscountDto
    {
        // Optional — references the original SalesOrderDiscount row (null for newly added discounts)
        public int? SalesOrderDiscountId { get; set; }

        public int DiscountMasterId { get; set; }
        public int SlabTypeId { get; set; }
        public int PaymentTermId { get; set; }
        public int? DiscountSlabId { get; set; }
        public decimal? DiscountRate { get; set; }
        public decimal? TotalDiscountValue { get; set; }
    }
}
