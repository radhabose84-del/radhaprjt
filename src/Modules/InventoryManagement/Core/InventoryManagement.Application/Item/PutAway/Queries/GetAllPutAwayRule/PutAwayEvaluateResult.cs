namespace InventoryManagement.Application.Item.PutAway.Queries.GetAllPutAwayRule
{
    public sealed class PutAwayEvaluateResult
    {
        public int RuleId { get; set; }
        public int StrategyId { get; set; }
        public int StorageTypeId { get; set; }
        public int? TargetId { get; set; }
        public string? LocationCode { get; set; }
    }
}