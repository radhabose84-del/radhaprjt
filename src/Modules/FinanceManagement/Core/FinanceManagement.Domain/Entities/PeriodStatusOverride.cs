using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // US-GL03-02 — dual-approval reversal audit for AccountingPeriod status changes
    // (HARDCLOSED → SOFTCLOSED or SOFTCLOSED → OPEN). Repointed to Finance.AccountingPeriod
    // after the FinancialPeriodMaster table was retired (refactor 2026-06-26).
    public class PeriodStatusOverride : BaseEntity
    {
        // Same-module FK -> Finance.AccountingPeriod (period being reversed).
        public int AccountingPeriodId { get; set; }
        public int CompanyId { get; set; }

        // FKs to MiscMaster (MiscTypeCode = 'FPS' for From/To statuses, 'PSO' for OverrideStatus).
        public int FromStatusId { get; set; }
        public int ToStatusId { get; set; }

        public int RequestedBy { get; set; }
        public DateTimeOffset RequestedAt { get; set; }
        public string? RequestedReason { get; set; }

        public int? CfoApproverId { get; set; }
        public DateTimeOffset? CfoApprovedAt { get; set; }

        public int? SysAdminApproverId { get; set; }
        public DateTimeOffset? SysAdminApprovedAt { get; set; }

        public int OverrideStatusId { get; set; }       // FK -> MiscMaster (PSO)
        public DateTimeOffset? AppliedAt { get; set; }
        public string? RejectionReason { get; set; }

        // Same-module navigation
        public AccountingPeriod? AccountingPeriod { get; set; }
        public MiscMaster? FromStatusMaster { get; set; }
        public MiscMaster? ToStatusMaster { get; set; }
        public MiscMaster? OverrideStatusMaster { get; set; }
    }
}
