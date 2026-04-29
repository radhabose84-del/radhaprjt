namespace SalesManagement.Domain.Entities
{
    public class SalesQuotationDetail
    {
        public int Id { get; set; }
        public int SalesQuotationHeaderId { get; set; }
        public SalesQuotationHeader SalesQuotationHeader { get; set; } = null!;
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
