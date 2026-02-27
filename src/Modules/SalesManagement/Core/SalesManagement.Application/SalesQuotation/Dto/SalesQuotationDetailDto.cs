namespace SalesManagement.Application.SalesQuotation.Dto
{
    public class SalesQuotationDetailDto
    {
        public int Id { get; set; }
        public int SalesQuotationHeaderId { get; set; }
        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public decimal Quantity { get; set; }
        public decimal ExMillRate { get; set; }
        public decimal Discount { get; set; }
        public decimal NetRate { get; set; }
        public decimal TotalAmount { get; set; }
        public int HSNId { get; set; }
        public string? HSNCode { get; set; }
        public string? HSNDescription { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal TaxAmount { get; set; }
    }
}
