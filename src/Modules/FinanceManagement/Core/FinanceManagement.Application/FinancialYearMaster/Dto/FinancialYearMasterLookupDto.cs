namespace FinanceManagement.Application.FinancialYearMaster.Dto
{
    public class FinancialYearMasterLookupDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string? FinancialYearCode { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int StatusId { get; set; }
        public string? StatusCode { get; set; }
    }
}
