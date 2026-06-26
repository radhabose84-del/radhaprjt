namespace Contracts.Dtos.Lookups.Finance
{
    /// <summary>
    /// US-GL03-01..05 (post-refactor 2026-06-26) — lightweight cross-module read of
    /// Finance.AccountingPeriod. Replaces the dropped FinancialPeriodMasterLookupDto.
    /// </summary>
    public sealed class AccountingPeriodLookupDto
    {
        public int Id { get; set; }
        public int FinancialYearId { get; set; }
        public string? FinancialYearName { get; set; }   // AppData.FinancialYear.FinYearName
        public int CompanyId { get; set; }
        public int PeriodNo { get; set; }                 // 1..13 (13 = adjustment)
        public string? PeriodName { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int StatusId { get; set; }
        public string? StatusCode { get; set; }           // 'OPEN' / 'SOFTCLOSED' / 'HARDCLOSED'  (FPS MiscType)
        public bool IsAdjustmentPeriod { get; set; }
    }
}
