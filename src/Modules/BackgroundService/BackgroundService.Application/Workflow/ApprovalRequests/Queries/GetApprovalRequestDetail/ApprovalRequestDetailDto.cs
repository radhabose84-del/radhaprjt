namespace BackgroundService.Application.Workflow.ApprovalRequests.Queries.GetApprovalRequestDetail
{
    public class ApprovalRequestDetailDto
    {
        public int Id { get; set; }
        public int ModuleTransactionId { get; set; }
        public string? WorkflowType { get; set; }
        public int WorkflowTypeId { get; set; }
        public int ApprovalStepDetailId { get; set; }
        public int? ApprovalRuleId { get; set; }
        public int StatusId { get; set; }
        public string? StatusCode { get; set; }
        public string? ApproverBinding { get; set; }
        public string? ApproverValue { get; set; }
        public DateTimeOffset RequestedDate { get; set; }
        public int UnitId { get; set; }
        public int? DepartmentId { get; set; }
        public string? Remark { get; set; }
        public string? ModuleTypeName { get; set; }
    }
}
