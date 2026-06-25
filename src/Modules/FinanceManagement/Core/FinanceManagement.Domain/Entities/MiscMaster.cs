using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    public class MiscMaster : BaseEntity
    {
        public int MiscTypeId { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }

        // Same-module FK navigation
        public MiscTypeMaster? MiscTypeMaster { get; set; }

        // Reverse navigation — GlAccountMaster uses MiscMaster twice (NormalBalance + SubLedgerType)
        public ICollection<GlAccountMaster>? GlAccountsAsNormalBalance { get; set; }
        public ICollection<GlAccountMaster>? GlAccountsAsSubLedgerType { get; set; }

        // Reverse navigation — FinancialYearMaster / FinancialPeriodMaster use MiscMaster
        // for Status (MiscTypeCode = 'FYS' for years; 'FPS' for periods)
        public ICollection<FinancialYearMaster>?   FinancialYearsAsStatus   { get; set; }
        public ICollection<FinancialPeriodMaster>? FinancialPeriodsAsStatus { get; set; }
    }
}
