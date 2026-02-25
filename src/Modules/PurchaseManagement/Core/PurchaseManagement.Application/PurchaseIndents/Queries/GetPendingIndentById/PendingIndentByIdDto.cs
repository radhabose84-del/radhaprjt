namespace PurchaseManagement.Application.PurchaseIndents.Queries.GetPendingIndentById
{
    public class PendingIndentByIdDto
    {
        public int Id { get; set; }
        public string IndentNumber { get; set; } = default!;
        public DateOnly IndentDate { get; set; }
        public int IndentTypeId { get; set; }
        public string IndentTypeName { get; set; } = default!;
        public int UnitId { get; set; }
        public string UnitName { get; set; } = default!;
        public string Purpose { get; set; } = default!;
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = default!;
        public int ApprovalRequestHeaderId { get; set; }
        public int ApproverId { get; set; }
        public string ApproverName { get; set; } = default!;
        // public string IsApprover { get; set; }
        public ICollection<PendingIndentDetailByIdDto> IndentDetails { get; set; } = default!;
        
    }
}