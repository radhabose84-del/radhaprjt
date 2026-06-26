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

        // Reverse navigation — PeriodStatusOverride uses MiscMaster three times
        //   FromStatusId / ToStatusId  (MiscTypeCode = 'FPS' — OPEN/SOFTCLOSED/HARDCLOSED)
        //   OverrideStatusId           (MiscTypeCode = 'PSO')
        public ICollection<PeriodStatusOverride>? PeriodStatusOverridesAsFrom         { get; set; }
        public ICollection<PeriodStatusOverride>? PeriodStatusOverridesAsTo           { get; set; }
        public ICollection<PeriodStatusOverride>? PeriodStatusOverridesAsOverrideState { get; set; }
    }
}
