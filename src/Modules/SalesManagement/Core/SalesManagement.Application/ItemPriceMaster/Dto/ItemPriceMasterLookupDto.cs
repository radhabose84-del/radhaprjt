namespace SalesManagement.Application.ItemPriceMaster.Dto
{
    public sealed class ItemPriceMasterLookupDto
    {
        public int Id { get; set; }
        public string? PriceCode { get; set; }
        public string? ItemName { get; set; }
        public decimal ExMillRate { get; set; }
        public DateOnly ValidFrom { get; set; }
        public DateOnly ValidTo { get; set; }
    }
}
