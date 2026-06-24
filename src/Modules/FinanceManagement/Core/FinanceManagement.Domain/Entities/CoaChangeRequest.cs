using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // US-GL02-08B — a request to change a sealed COA. Carries a CFO-approved impact assessment (AC5)
    // before it can be attached to a dual-approval unfreeze window (CoaUnfreezeRequest). When the change
    // is committed during an open window it is flagged Post-Freeze (AC3); if the window lapses with the
    // change still uncommitted the request is cancelled as 'Lapsed' (AC4).
    public class CoaChangeRequest : BaseEntity, IAuditTrailed
    {
        public int CompanyId { get; set; }

        // What the change targets — an account and/or a group. At least one is set (the other NULL).
        public int? TargetAccountId { get; set; }
        public int? TargetAccountGroupId { get; set; }

        // Snapshot of the account/group code at raise time — kept on the post-freeze log so the report
        // is stable even if the code later changes.
        public string? AccountCodeSnapshot { get; set; }

        // AccountEdit / CodeChange / GroupMove / Deactivate / FsRemap (see CoaChangeType).
        public string ChangeType { get; set; } = null!;

        public string Justification { get; set; } = null!;

        // AC5 — mandatory impact assessment that the CFO must approve before any unfreeze approval proceeds.
        public string ImpactAssessment { get; set; } = null!;
        public int? ImpactApprovedByUserId { get; set; }
        public DateTimeOffset? ImpactApprovedOn { get; set; }

        // The dual-approval unfreeze window this request is batched under (NULL until attached). One window
        // can carry many change requests (AC4 speaks of plural 'incomplete change requests').
        public int? UnfreezeRequestId { get; set; }
        public CoaUnfreezeRequest? UnfreezeRequest { get; set; }

        // PendingImpactApproval / ImpactApproved / Committed / Lapsed / Rejected (see CoaChangeRequestStatus).
        // Named RequestStatus to avoid hiding BaseEntity.Status.
        public string RequestStatus { get; set; } = CoaChangeRequestStatus.PendingImpactApproval;

        // AC3 — set true when the change is committed during an open window; this is the flag COA reports read.
        public bool IsPostFreeze { get; set; }
        public int? CommittedByUserId { get; set; }
        public DateTimeOffset? CommittedOn { get; set; }

        public int RequestedByUserId { get; set; }
        public DateTimeOffset? RequestedOn { get; set; }
    }
}
