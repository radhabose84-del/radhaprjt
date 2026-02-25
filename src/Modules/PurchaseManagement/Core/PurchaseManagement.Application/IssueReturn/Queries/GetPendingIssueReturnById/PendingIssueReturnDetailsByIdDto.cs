namespace PurchaseManagement.Application.IssueReturn.Queries.GetPendingIssueReturnById
{
    public class PendingIssueReturnDetailsByIdDto
    {
        public int Id { get; set; }
        public int IssueReturnHeaderId { get; set; }
        public int ApprovalRequestLineId { get; set; }
        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public int UomId { get; set; }
        public string? UOMName { get; set; }
        public int WarehouseStockId { get; set; }
        public int StorageTypeId { get; set; }
        public int TargetId { get; set; }
        public decimal ReturnQuantity { get; set; }
        public decimal ReturnValue { get; set; }
        public int ReasonId { get; set; }
        public string? ReasonName { get; set; }
        public int SubStoresDepartmentId { get; set; }
        public string? SubStoresDepartmentName { get; set; }
         // ⭐ ADD THIS
        public List<PutawayRuleDisplayDto>? PutawayRules { get; set; }
    }
}