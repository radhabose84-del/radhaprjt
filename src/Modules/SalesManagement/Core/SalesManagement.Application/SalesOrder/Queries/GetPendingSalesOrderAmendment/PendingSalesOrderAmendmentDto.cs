namespace SalesManagement.Application.SalesOrder.Queries.GetPendingSalesOrderAmendment
{
    public class PendingSalesOrderAmendmentDto
    {
        public int Id { get; set; }
        public int SalesOrderHeaderId { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public string? SalesOrderNo { get; set; }
        public string? AmendmentNo { get; set; }
        public int RevisionNumber { get; set; }
        public DateOnly AmendmentDate { get; set; }
        public string? Reason { get; set; }
        public int? StatusId { get; set; }
        public string? StatusName { get; set; }
        public string? CreatedByName { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }

        // Workflow fields
        public int ApproverId { get; set; }
        public string? ApproverName { get; set; }
        public int ApprovalRequestHeaderId { get; set; }
        public byte IsEdit { get; set; }
    }
}
