namespace FinanceManagement.Domain.Entities
{
    // String status constants for the US-GL02-08B change-request / unfreeze workflow. Stored as the
    // "Status" varchar column (same convention as AccountGroupChangeRequest) so the values are readable
    // in COA reports and the post-freeze change log.

    public static class CoaChangeRequestStatus
    {
        public const string PendingImpactApproval = "PendingImpactApproval";
        public const string ImpactApproved = "ImpactApproved";
        public const string Committed = "Committed";
        public const string Lapsed = "Lapsed";
        public const string Rejected = "Rejected";
    }

    public static class CoaUnfreezeRequestStatus
    {
        public const string PendingApproval = "PendingApproval";
        public const string WindowOpen = "WindowOpen";
        public const string Expired = "Expired";
        public const string Cancelled = "Cancelled";
    }

    // The kinds of structural change a request can describe (AC3 "change type").
    public static class CoaChangeType
    {
        public const string AccountEdit = "AccountEdit";
        public const string CodeChange = "CodeChange";
        public const string GroupMove = "GroupMove";
        public const string Deactivate = "Deactivate";
        public const string FsRemap = "FsRemap";
    }
}
