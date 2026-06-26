namespace FinanceManagement.Application.FinancialYearMaster.Dto
{
    public class FinancialPeriodMasterDto
    {
        public int Id { get; set; }
        public int FinancialYearId { get; set; }
        public string? FinancialYearCode { get; set; }
        public int CompanyId { get; set; }
        public byte PeriodNumber { get; set; }
        public string? PeriodName { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public int StatusId { get; set; }
        public string? StatusCode { get; set; }    // 'OPEN' / 'SOFTCLOSED' / 'HARDCLOSED'
        public string? StatusName { get; set; }    // 'Open' / 'Soft Closed' / 'Hard Closed'

        public bool IsAdjustmentPeriod { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}
