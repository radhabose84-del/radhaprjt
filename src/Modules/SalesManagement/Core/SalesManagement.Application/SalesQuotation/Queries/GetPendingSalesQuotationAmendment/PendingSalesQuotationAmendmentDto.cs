namespace SalesManagement.Application.SalesQuotation.Queries.GetPendingSalesQuotationAmendment
{
    public class PendingSalesQuotationAmendmentDto
    {
        public int Id { get; set; }
        public int SalesQuotationHeaderId { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public string? QuotationNo { get; set; }
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
