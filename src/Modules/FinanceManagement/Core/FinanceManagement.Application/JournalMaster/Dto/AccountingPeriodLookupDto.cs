namespace FinanceManagement.Application.JournalMaster.Dto
{
    public sealed class AccountingPeriodLookupDto
    {
        public int Id { get; set; }
        public int PeriodNo { get; set; }
        public string? PeriodName { get; set; }
    }
}
