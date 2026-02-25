namespace PurchaseManagement.Application.IssueReturn.Queries.GetPendingIssueReturnById
{
    public class PendingIssueReturnByIdDto
    {
         public int IssueReturnId { get; set; }
        public string? IssueReturnNo { get; set; }
        public DateTimeOffset IssueReturnDate { get; set; }
        public int RequestCategoryId { get; set; }
        public string? RequestCategoryName { get; set; }
        public int? IssueHeaderId { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public string? Remarks { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int ApprovalRequestHeaderId { get; set; }
        public int ApproverId { get; set; }
        public string? ApproverName { get; set; }
        // public string IsApprover { get; set; }
        public ICollection<PendingIssueReturnDetailsByIdDto>? PendingIssueReturnDetails { get; set; }
    }
}