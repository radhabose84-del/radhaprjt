namespace FinanceManagement.Application.Common.Options
{
    // US-GL02-08B — configurable role mapping + window default (bound from the "CoaUnfreeze" config section).
    // The CFO / System Admin / FC / Internal Audit roles are NOT seeded; the deployment maps them to the
    // RoleIds that exist in AppSecurity.UserRole. The dual-approval slots and alert recipients resolve
    // against these IDs via IRoleUserLookup.
    public class CoaUnfreezeOptions
    {
        public const string SectionName = "CoaUnfreeze";

        public int CfoRoleId { get; set; }
        public int SystemAdminRoleId { get; set; }
        public int FcRoleId { get; set; }
        public int InternalAuditRoleId { get; set; }

        // Length of the unfreeze window granted on dual approval (08A auto-re-freezes after this).
        public int DefaultWindowMinutes { get; set; } = 60;
    }
}
