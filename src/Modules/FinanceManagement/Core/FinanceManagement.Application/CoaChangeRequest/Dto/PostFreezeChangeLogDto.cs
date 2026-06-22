namespace FinanceManagement.Application.CoaChangeRequest.Dto
{
    // US-GL02-08B (AC3) — the post-freeze change log: each committed change with the account, change type,
    // BOTH approvers + timestamps, and justification, flagged 'Post-Freeze'. Built as a read-model joining
    // committed CoaChangeRequest rows to their CoaUnfreezeRequest. Approver names are enriched via IUserLookup.
    public class PostFreezeChangeLogDto
    {
        public int ChangeRequestId { get; set; }
        public int CompanyId { get; set; }
        public int? TargetAccountId { get; set; }
        public int? TargetAccountGroupId { get; set; }
        public string? AccountCode { get; set; }
        public string? ChangeType { get; set; }
        public string? Justification { get; set; }

        public int? CfoApproverUserId { get; set; }
        public string? CfoApproverName { get; set; }
        public DateTimeOffset? CfoApprovedOn { get; set; }

        public int? SysAdminApproverUserId { get; set; }
        public string? SysAdminApproverName { get; set; }
        public DateTimeOffset? SysAdminApprovedOn { get; set; }

        public int? CommittedByUserId { get; set; }
        public DateTimeOffset? CommittedOn { get; set; }

        // Always true in this read-model — the report column that marks the change 'Post-Freeze'.
        public bool IsPostFreeze { get; set; }
    }
}
