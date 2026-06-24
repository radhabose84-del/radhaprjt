namespace FinanceManagement.Application.JournalMaster.Dto
{
    // Read model for the period ledger balance enriched with its GL account, account type and account group
    // (all same-module → SQL JOINs).
    public sealed class LedgerBalanceDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }

        // GL account
        public int GlAccountId { get; set; }
        public string? AccountCode { get; set; }
        public string? AccountName { get; set; }

        // Account type
        public int AccountTypeId { get; set; }
        public string? AccountTypeName { get; set; }

        // Account group (+ hierarchy)
        public int AccountGroupId { get; set; }
        public string? GroupCode { get; set; }
        public string? GroupName { get; set; }
        public int GroupLevel { get; set; }                 // AccountGroup.Level (hierarchy depth)
        public bool IsLeaf { get; set; }
        public int? ParentAccountGroupId { get; set; }
        public string? ParentGroupCode { get; set; }
        public string? ParentGroupName { get; set; }

        // Period / FY / cost centre
        public int AccountingPeriodId { get; set; }
        public string? PeriodName { get; set; }             // AccountingPeriod name
        public int FinancialYearId { get; set; }
        public string? FinancialYearName { get; set; }      // cross-module — populated via IFinancialYearLookup
        public int? CostCentreId { get; set; }
        public string? CostCentreName { get; set; }

        // Balances
        public decimal DrTotal { get; set; }
        public decimal CrTotal { get; set; }
        public decimal Balance { get; set; }
    }
}
