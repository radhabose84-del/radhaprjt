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

        // Approval workflow ModuleTypeName / Menu name for journal voucher approval (US-GL01-06B).
        // MUST equal the configured Menu / WorkflowType name so the workflow resolves it.
        public const string JournalVoucher = "Journal Voucher";

        // Approval workflow ModuleTypeName / Menu name for recurring journal template approval (US-GL01-11).
        public const string RecurringJournalTemplate = "Recurring Journal";

        // ApprovalStatus misc-type code + lifecycle status values.
        public const string ApprovalStatus = "ApprovalStatus";
        public const string Pending = "Pending";
    }
}
