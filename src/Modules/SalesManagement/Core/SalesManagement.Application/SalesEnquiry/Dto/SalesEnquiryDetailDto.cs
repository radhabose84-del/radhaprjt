namespace SalesManagement.Application.SalesEnquiry.Dto
{
    public class SalesEnquiryDetailDto
    {
        public int Id { get; set; }
        public int SalesEnquiryHeaderId { get; set; }
        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public int? VariantId { get; set; }
        public string? VariantName { get; set; }
        public decimal Quantity { get; set; }
        public decimal? ExmillRate { get; set; }
        public decimal? TargetPrice { get; set; }
        public decimal? Discount { get; set; }
    }
}
