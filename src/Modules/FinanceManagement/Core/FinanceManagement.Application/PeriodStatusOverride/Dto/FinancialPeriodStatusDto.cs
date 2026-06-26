namespace FinanceManagement.Application.PeriodStatusOverride.Dto
{
    public class FinancialPeriodStatusDto
    {
        public int PeriodId { get; set; }
        public int FinancialYearId { get; set; }
        public string? FinancialYearCode { get; set; }
        public int CompanyId { get; set; }
        public byte PeriodNumber { get; set; }
        public string? PeriodName { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public int StatusId { get; set; }
        public string? StatusCode { get; set; }       // 'OPEN' / 'SOFTCLOSED' / 'HARDCLOSED'
        public string? StatusName { get; set; }
        public bool IsAdjustmentPeriod { get; set; }

        public int? LastStatusChangedBy { get; set; }
        public DateTimeOffset? LastStatusChangedAt { get; set; }
    }
}
