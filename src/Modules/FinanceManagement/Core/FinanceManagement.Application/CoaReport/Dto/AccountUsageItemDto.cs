namespace FinanceManagement.Application.CoaReport.Dto
{
    // US-GL02-15 (AC2/AC3) — usage row for accounts that are never posted or have had no posting in the
    // window. IsDeactivationCandidate is false for balance-sheet accounts that still carry a balance.
    public class AccountUsageItemDto
    {
        public int Id { get; set; }
        public string? AccountCode { get; set; }
        public string? AccountName { get; set; }
        public string? AccountTypeName { get; set; }
        public string? GroupName { get; set; }
        public bool IsActive { get; set; }

        public int PostingCount { get; set; }
        public bool NeverPosted { get; set; }
        public DateOnly? LastPostingDate { get; set; }
        public int? MonthsSincePosting { get; set; }     // null when never posted

        public string? StatementTypeCode { get; set; }   // 'BS' / 'PL' / null (unmapped)
        public bool IsBalanceSheet { get; set; }
        public decimal Balance { get; set; }

        // AC3 — true only when the account may safely be proposed for deactivation.
        public bool IsDeactivationCandidate { get; set; }
        public string? ExclusionReason { get; set; }     // why it is NOT a candidate (e.g. BS with balance)
    }
}
