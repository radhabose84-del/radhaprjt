namespace FinanceManagement.Application.JournalMaster.Dto
{
    public class AccountingPeriodDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }            // populated via ICompanyLookup
        public int FinancialYearId { get; set; }
        public string? FinancialYearName { get; set; }      // populated via lookup
        public string? PeriodName { get; set; }
        public int PeriodNo { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }             // MiscMaster (PERIOD_STATUS)

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}
