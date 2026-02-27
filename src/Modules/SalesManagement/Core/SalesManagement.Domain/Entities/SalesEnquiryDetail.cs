namespace SalesManagement.Domain.Entities
{
    public class SalesEnquiryDetail
    {
        public int Id { get; set; }
        public int SalesEnquiryHeaderId { get; set; }
        public SalesEnquiryHeader SalesEnquiryHeader { get; set; } = null!;
        public int ItemId { get; set; }
        public decimal Quantity { get; set; }
        public decimal? ExmillRate { get; set; }
        public decimal? TargetPrice { get; set; }
        public decimal? Discount { get; set; }
    }
}
