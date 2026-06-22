namespace FinanceManagement.Domain.Entities
{
    // US-GL01-09 — period balances per account and cost centre. A running aggregate (NOT a BaseEntity
    // master): no soft delete / audit fields. Updated atomically during posting under DB-level
    // concurrency control (RowVersion) so simultaneous posts cannot corrupt a balance.
    public class LedgerBalance
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }              // cross-module — no DB constraint
        public int GlAccountId { get; set; }            // same-module FK -> GlAccountMaster
        public int AccountingPeriodId { get; set; }     // same-module FK -> AccountingPeriod
        public int? CostCentreId { get; set; }          // same-module FK -> CostCentre (NULL = no cost centre)
        public int FinancialYearId { get; set; }        // cross-module — no DB constraint

        public decimal DrTotal { get; set; }
        public decimal CrTotal { get; set; }
        public decimal Balance { get; set; }            // DrTotal - CrTotal

        // Persisted computed key so a single "no cost centre" row is unique per account/period.
        public int CostCentreKey { get; private set; }

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();   // optimistic-concurrency token (US-09 AC-2)

        // Same-module FK navigation
        public GlAccountMaster? GlAccount { get; set; }
        public AccountingPeriod? AccountingPeriod { get; set; }
        public CostCentre? CostCentre { get; set; }
    }
}
