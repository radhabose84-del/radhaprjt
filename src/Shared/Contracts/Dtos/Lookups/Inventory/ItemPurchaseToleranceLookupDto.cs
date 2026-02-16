namespace Contracts.Dtos.Lookups.Inventory
{
    public class ItemPurchaseToleranceLookupDto
    {
        public int ItemId { get; init; }
        public string? ItemCode { get; init; }
        public string? ItemName { get; init; }
        public decimal LowerTolerance { get; init; }
        public decimal UpperTolerance { get; init; }
        public int? PurchaseUomId { get; init; }
        public string? UOMName { get; init; }
    }
}
