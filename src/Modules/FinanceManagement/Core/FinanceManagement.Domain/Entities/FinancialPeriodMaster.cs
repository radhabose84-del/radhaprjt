using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    public class FinancialPeriodMaster : BaseEntity
    {
        public int FinancialYearId { get; set; }
        public int CompanyId { get; set; }                  // denormalised for posting-engine hot path
        public byte PeriodNumber { get; set; }              // 1..13
        public string? PeriodName { get; set; }             // "Apr-2024" / "Adj-2024-25"
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        // FK to Finance.MiscMaster where MiscTypeCode = 'FPS' (Open / SoftClosed / HardClosed)
        public int StatusId { get; set; }

        public bool IsAdjustmentPeriod { get; set; }        // true for Period 13

        // US-GL03-02 — denormalised audit-trail snapshot for fast /status reads
        public int? LastStatusChangedBy { get; set; }
        public DateTimeOffset? LastStatusChangedAt { get; set; }

        // Same-module FK navigation
        public FinancialYearMaster? FinancialYear { get; set; }
        public MiscMaster? StatusMaster { get; set; }

        // Reverse navigation — US-GL03-02 override history
        public ICollection<PeriodStatusOverride>? Overrides { get; set; }
    }
}
