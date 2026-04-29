namespace SalesManagement.Application.SalesQuotation.Dto
{
    public class CreateSalesQuotationDetailDto
    {
        public int ItemId { get; set; }
        public int? VariantId { get; set; }
        public decimal Quantity { get; set; }
        public int? UOMId { get; set; }
        public decimal ExMillRate { get; set; }
        public decimal Discount { get; set; }
        public int? DiscountTypeId { get; set; }
        public decimal NetRate { get; set; }
        public decimal TotalAmount { get; set; }
        public int HSNId { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal TaxAmount { get; set; }
    }
}
