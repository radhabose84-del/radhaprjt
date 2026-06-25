namespace FinanceManagement.Application.CoaReport.Dto
{
    // US-GL02-15 (AC1/AC5) — one row per GL account for the COA listing (hierarchy + attributes +
    // active status + posting count + FS-mapping). Ordered by AccountCode for the PDF.
    public class CoaListingItemDto
    {
        public int Id { get; set; }
        public string? AccountCode { get; set; }
        public string? AccountName { get; set; }

        public int AccountTypeId { get; set; }
        public string? AccountTypeName { get; set; }

        public int AccountGroupId { get; set; }
        public string? GroupCode { get; set; }
        public string? GroupName { get; set; }
        public int GroupLevel { get; set; }
        public string? ParentGroupCode { get; set; }
        public string? ParentGroupName { get; set; }

        public string? NormalBalanceCode { get; set; }
        public string? SubLedgerTypeCode { get; set; }

        public bool IsCostCentreMandatory { get; set; }
        public bool IsProfitCentreMandatory { get; set; }
        public bool IsTaxRelevant { get; set; }
        public bool IsInterCompany { get; set; }
        public bool IsReconciliationRequired { get; set; }
        public bool IsActive { get; set; }

        // FS-mapping (Schedule III) — null line when the account's group is unmapped.
        public int? ScheduleIIISectionItemId { get; set; }
        public string? FsLineCode { get; set; }
        public string? FsLineName { get; set; }
        public string? StatementTypeCode { get; set; }   // 'BS' / 'PL'

        public int PostingCount { get; set; }
        public DateOnly? LastPostingDate { get; set; }
        public decimal Balance { get; set; }
    }
}
