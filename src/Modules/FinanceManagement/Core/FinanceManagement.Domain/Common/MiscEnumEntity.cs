namespace FinanceManagement.Domain.Common
{
    public static class MiscEnumEntity
    {
        public const string FinanceDocument = "FinanceDocument";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";

        // Approval workflow ModuleTypeName — MUST equal the Menu name (MenuId 1288).
        public const string AccountGroupHierarchy = "Account Group Hierarchy";

        // AccountGroupChangeRequest lifecycle statuses.
        public const string Pending = "Pending";
    }
}
