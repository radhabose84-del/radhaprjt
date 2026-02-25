namespace InventoryManagement.Application.Item.PutAway.Commands.CreatePutAwayRule
{
    public sealed class CreatePutAwayRuleRequest
    {
        public int UnitId { get; set; }
        public int ItemGroupId { get; set; }
        public int ItemCategoryId { get; set; }
        public int? ItemId { get; set; }
        public int WarehouseId { get; set; }
        public byte IsActive { get; set; }
        public List<CreatePutAwayStrategyRequest> Strategies { get; set; } = new();
    }
}