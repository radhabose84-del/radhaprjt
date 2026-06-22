namespace FinanceManagement.Application.AccountGroup.Dto
{
    // One pending Account Group Move awaiting the logged-in approver (FC at L1, CFO at L2).
    // The approval-page inbox row: who/what is moving, from→to, and the ids needed to approve/reject.
    public class AccountGroupMovePendingDto
    {
        public int ChangeRequestId { get; set; }

        // = ModuleTransactionId in the approval engine.
        public int AccountGroupId { get; set; }
        public string? GroupCode { get; set; }
        public string? GroupName { get; set; }

        public int? CurrentParentId { get; set; }
        public string? CurrentParentCode { get; set; }
        public string? CurrentParentName { get; set; }

        public int NewParentAccountGroupId { get; set; }
        public string? NewParentCode { get; set; }
        public string? NewParentName { get; set; }

        public string? Justification { get; set; }

        public int RequestedById { get; set; }
        public string? RequestedByName { get; set; }
        public DateTimeOffset? RequestedDate { get; set; }

        // ── Filled from the approval engine (IWorkflowLookup) for the current step ──
        public int ApprovalRequestHeaderId { get; set; }   // pass to the approve/reject endpoint
        public int ApproverId { get; set; }                // current step approver (= logged-in user)
        public byte IsEdit { get; set; }
    }
}
