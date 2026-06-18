namespace FinanceManagement.Application.GlAccountImport.Dto
{
    /// <summary>
    /// One raw row from an uploaded file (parser output) or one row destined for export.
    /// All values are kept as trimmed strings — resolution to ids/bools happens in the validator.
    /// </summary>
    public sealed class GlAccountImportRowDto
    {
        /// <summary>1-based row number as it appears in the file (header = row 1).</summary>
        public int RowNumber { get; set; }

        public string? RecordType { get; set; }

        // GROUP fields
        public string? GroupCode { get; set; }
        public string? GroupName { get; set; }
        public string? ParentGroupCode { get; set; }
        public string? AccountType { get; set; }
        public string? SortOrder { get; set; }

        // ACCOUNT fields
        public string? AccountCode { get; set; }
        public string? AccountName { get; set; }
        public string? Description { get; set; }
        public string? AccountGroupCode { get; set; }
        public string? NormalBalance { get; set; }
        public string? Currency { get; set; }
        public string? SubLedgerType { get; set; }
        public string? IsCostCentreMandatory { get; set; }
        public string? IsTaxRelevant { get; set; }
        public string? IsInterCompany { get; set; }
        public string? IsReconciliationRequired { get; set; }
    }
}
