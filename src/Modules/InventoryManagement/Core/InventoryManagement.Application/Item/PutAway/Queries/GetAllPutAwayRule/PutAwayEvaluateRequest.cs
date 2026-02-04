namespace InventoryManagement.Application.Item.PutAway.Queries.GetAllPutAwayRule
{
    public sealed class PutAwayEvaluateRequest
    {
        public int UnitId { get; set; }
        public int WarehouseId { get; set; }
        public int ItemGroupId { get; set; }
        public int ItemCategoryId { get; set; }
        public int? ItemId { get; set; }
        public decimal? Quantity { get; set; }
    }
}