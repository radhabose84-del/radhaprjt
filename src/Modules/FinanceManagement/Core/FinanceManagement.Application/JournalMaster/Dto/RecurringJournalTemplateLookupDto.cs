namespace FinanceManagement.Application.JournalMaster.Dto
{
    public sealed class RecurringJournalTemplateLookupDto
    {
        public int Id { get; set; }
        public string? TemplateName { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }         // ApprovalStatus (Pending / Approved / Rejected)
    }
}
