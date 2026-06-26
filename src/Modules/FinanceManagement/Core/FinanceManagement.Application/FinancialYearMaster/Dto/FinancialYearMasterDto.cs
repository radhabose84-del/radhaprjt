namespace FinanceManagement.Application.FinancialYearMaster.Dto
{
    public class FinancialYearMasterDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string? FinancialYearCode { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public int StatusId { get; set; }
        public string? StatusCode { get; set; }     // 'OPEN' / 'CLOSED'
        public string? StatusName { get; set; }     // 'Open' / 'Closed'

        public bool IsTransitionYear { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public List<FinancialPeriodMasterDto> Periods { get; set; } = new();

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
