namespace SalesManagement.Application.SalesOrder.Queries.GetPendingSalesOrder
{
    public class PendingSalesOrderDto
    {
        // Header fields
        public int Id { get; set; }
        public string? SalesOrderNo { get; set; }
        public DateOnly OrderDate { get; set; }
        public int SalesGroupId { get; set; }
        public string? SalesGroupName { get; set; }
        public int? SalesSegmentId { get; set; }
        public string? SegmentName { get; set; }
        public int EnquiryType { get; set; }
        public string? EnquiryTypeName { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public int PartyId { get; set; }
        public string? PartyName { get; set; }
        public int? AgentId { get; set; }
        public string? AgentName { get; set; }
        public int? SubAgentId { get; set; }
        public string? SubAgentName { get; set; }
        public int? StatusId { get; set; }
        public string? StatusName { get; set; }
        public string? Remarks { get; set; }

        // Summary fields
        public int TotalBags { get; set; }
        public decimal TotalWeightKgs { get; set; }
        public decimal FinalAmount { get; set; }

        // Audit
        public string? CreatedByName { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }

        // Workflow fields
        public int ApproverId { get; set; }
        public string? ApproverName { get; set; }
        public int ApprovalRequestHeaderId { get; set; }
        public byte IsEdit { get; set; }
    }
}
