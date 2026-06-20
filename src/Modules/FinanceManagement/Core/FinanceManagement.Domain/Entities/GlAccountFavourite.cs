using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // Per-user starred GL account for the type-ahead (US-GL02-07). One active row per
    // (UserId, CompanyId, GlAccountMasterId); un-starring is a soft delete.
    public class GlAccountFavourite : BaseEntity
    {
        public int GlAccountMasterId { get; set; }
        public int UserId { get; set; }      // AppSecurity.Users.UserId (cross-module — no DB FK)
        public int CompanyId { get; set; }   // cross-module — no DB FK

        // Same-module FK navigation → Finance.GlAccountMaster
        public GlAccountMaster? GlAccountMaster { get; set; }
    }
}
