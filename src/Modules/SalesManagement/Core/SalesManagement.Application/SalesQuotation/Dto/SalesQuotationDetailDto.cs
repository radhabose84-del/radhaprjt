namespace SalesManagement.Application.SalesQuotation.Dto
{
    public class SalesQuotationDetailDto
    {
        public int Id { get; set; }
        public int SalesQuotationHeaderId { get; set; }
        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public int? VariantId { get; set; }
        public string? VariantName { get; set; }
        public decimal Quantity { get; set; }
        public int? UOMId { get; set; }
        public string? UOMCode { get; set; }
        public string? UOMName { get; set; }
        public decimal ExMillRate { get; set; }
        public decimal Discount { get; set; }
        public int? DiscountTypeId { get; set; }
        public string? DiscountTypeCode { get; set; }
        public string? DiscountTypeDescription { get; set; }
        public decimal NetRate { get; set; }
        public decimal TotalAmount { get; set; }
        public int HSNId { get; set; }
        public string? HSNCode { get; set; }
        public string? HSNDescription { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal TaxAmount { get; set; }
    }
}
