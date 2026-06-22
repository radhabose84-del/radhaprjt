using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // US-GL02-08B — the dual-approval gateway that opens a time-boxed unfreeze window. Two DISTINCT
    // approvers are required: a CFO and a System Admin (AC1/AC2). When both slots are filled the handler
    // drives 08A's freeze state open (OpenUnfreezeWindowAsync) and alerts CFO/FC/Internal Audit. The 08A
    // auto-re-freeze job re-seals on WindowExpiry; the lapsing job then cancels any still-open requests.
    public class CoaUnfreezeRequest : BaseEntity
    {
        public int CompanyId { get; set; }

        public string Reason { get; set; } = null!;

        // The two distinct approval slots. AC1 is enforced by requiring CfoApproverUserId != SysAdminApproverUserId.
        public int? CfoApproverUserId { get; set; }
        public DateTimeOffset? CfoApprovedOn { get; set; }
        public int? SysAdminApproverUserId { get; set; }
        public DateTimeOffset? SysAdminApprovedOn { get; set; }

        // PendingApproval / WindowOpen / Expired / Cancelled (see CoaUnfreezeRequestStatus).
        public string RequestStatus { get; set; } = CoaUnfreezeRequestStatus.PendingApproval;

        // Length of the window granted on activation, and the resulting open/expiry stamps.
        public int WindowMinutes { get; set; }
        public DateTimeOffset? WindowOpenedOn { get; set; }
        public DateTimeOffset? WindowExpiry { get; set; }

        public int RequestedByUserId { get; set; }
        public DateTimeOffset? RequestedOn { get; set; }

        // Change requests batched under this window.
        public ICollection<CoaChangeRequest>? ChangeRequests { get; set; }
    }
}
