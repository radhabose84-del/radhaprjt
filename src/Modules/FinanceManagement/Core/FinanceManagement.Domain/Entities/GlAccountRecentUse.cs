using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // Per-user recently-used GL account for the type-ahead (US-GL02-07). One row per
    // (UserId, CompanyId, GlAccountMasterId), upserted on each select (LastUsedDate + UseCount++).
    public class GlAccountRecentUse : BaseEntity
    {
        public int GlAccountMasterId { get; set; }
        public int UserId { get; set; }      // AppSecurity.Users.UserId (cross-module — no DB FK)
        public int CompanyId { get; set; }   // cross-module — no DB FK

        public DateTimeOffset LastUsedDate { get; set; }
        public int UseCount { get; set; }

        // Same-module FK navigation → Finance.GlAccountMaster
        public GlAccountMaster? GlAccountMaster { get; set; }
    }
}
