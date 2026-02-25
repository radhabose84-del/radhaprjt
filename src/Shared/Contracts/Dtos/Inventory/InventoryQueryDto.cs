namespace Contracts.Dtos.Inventory
{
    public class InventoryQueryDto
    {
        public int ItemId { get; init; }
        public decimal LowerTolerance { get; init; }
        public decimal UpperTolerance { get; init; }
        public int? PurchaseUomId { get; init; }
        public string? UOMName { get; init; }

        public string? ItemCode { get; init; }
        public string? ItemName { get; init; }
    }
}