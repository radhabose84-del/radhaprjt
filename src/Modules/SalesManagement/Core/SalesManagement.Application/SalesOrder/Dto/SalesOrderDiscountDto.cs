namespace SalesManagement.Application.SalesOrder.Dto
{
    public class SalesOrderDiscountDto
    {
        public int Id { get; set; }
        public int DiscountMasterId { get; set; }
        public string? DiscountCode { get; set; }
        public string? DiscountName { get; set; }
        public int SlabTypeId { get; set; }
        public string? SlabTypeName { get; set; }
        public int PaymentTermId { get; set; }
        public string? PaymentTermDescription { get; set; }
        public int? DiscountSlabId { get; set; }
        public decimal? DiscountRate { get; set; }
        public decimal? TotalDiscountValue { get; set; }
    }
}
