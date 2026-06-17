namespace FinanceManagement.Domain.Common
{
    public static class MiscEnumEntity
    {
        public const string FinanceDocument = "FinanceDocument";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";

        // Approval workflow ModuleTypeName — MUST equal the Menu name (MenuId 1288).
        public const string AccountGroupHierarchy = "Account Group Hierarchy";

        // Approval workflow ModuleTypeName for tax-account linkage change requests —
        // MUST equal the configured Menu / WorkflowType name so sp_EvaluateApproval resolves it.
        public const string TaxAccountLinkage = "Tax Linkage";

        // ApprovalStatus misc-type code + lifecycle status values.
        public const string ApprovalStatus = "ApprovalStatus";
        public const string Pending = "Pending";
    }
}
