namespace SalesManagement.Application.ItemPriceMaster.Dto
{
    public class ExMillRateDto
    {
        public int Id { get; set; }
        public string? PriceCode { get; set; }
        public int SalesSegmentId { get; set; }
        public string? SalesSegmentName { get; set; }
        public decimal ExMillRate { get; set; }
    }
}
