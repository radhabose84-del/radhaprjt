namespace PurchaseManagement.Application.IssueReturn.Queries.GetPendingIssueReturnById
{
    public class PutawayRuleDisplayDto
    {
        public int? WarehouseId { get; set; }
        public string? WarehouseCode { get; set; }
        public string? WarehouseName { get; set; }
        public int StorageTypeId { get; set; }
        public string? StorageTypeName { get; set; }
        public int TargetId { get; set; }
        public string? TargetCode { get; set; }
        public string? TargetName { get; set; }
        public int PriorityId { get; set; }
        public string? PriorityName { get; set; }
    }
}