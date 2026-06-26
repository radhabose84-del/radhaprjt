namespace FinanceManagement.Application.Common.Options
{
    /// <summary>
    /// US-GL03-04 / AC#4 — configurable role mapping + scheduling for the weekly backdated-journal
    /// digest email (sent every Monday). RoleIds map to AppSecurity.UserRole rows; if both are 0
    /// the job skips sending (useful for environments where the digest is not yet wired up).
    /// </summary>
    public class BackdateDigestOptions
    {
        public const string SectionName = "BackdateDigest";

        public int CfoRoleId { get; set; }
        public int FcRoleId { get; set; }

        // How many days back to scan when the job fires (default: 7).
        public int WindowDays { get; set; } = 7;

        // Optional fixed cc list — useful for compliance / internal-audit.
        public List<string> CcEmails { get; set; } = new();
    }
}
