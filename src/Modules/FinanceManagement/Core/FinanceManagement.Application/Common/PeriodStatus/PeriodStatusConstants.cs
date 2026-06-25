namespace FinanceManagement.Application.Common.PeriodStatus
{
    /// <summary>
    /// Canonical MiscMaster.Code values used by the period-status state machine
    /// and the override workflow. Match the seeded rows under MiscTypeCode = 'FPS' and 'PSO'.
    /// </summary>
    public static class PeriodStatusConstants
    {
        // MiscTypeCode = 'FPS' — period lifecycle
        public const string Open       = "OPEN";
        public const string SoftClosed = "SOFTCLOSED";
        public const string HardClosed = "HARDCLOSED";

        // MiscTypeCode = 'PSO' — override workflow
        public const string OverridePending       = "PENDING";
        public const string OverrideFullyApproved = "FULLYAPPROVED";
        public const string OverrideApplied       = "APPLIED";
        public const string OverrideRejected      = "REJECTED";
    }
}
