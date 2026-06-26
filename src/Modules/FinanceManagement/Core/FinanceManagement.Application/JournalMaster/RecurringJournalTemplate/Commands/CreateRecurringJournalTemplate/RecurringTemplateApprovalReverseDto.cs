namespace FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.CreateRecurringJournalTemplate
{
    // Approval-workflow payload wrapper. sp_EvaluateApproval reads $.Header.UnitId.
    public sealed class RecurringTemplateApprovalReverseDto
    {
        public RecurringTemplateApprovalHeaderDto? Header { get; set; }
        public object? Lines { get; set; }   // mirrors the journal approval payload; always null for templates
    }

    public sealed class RecurringTemplateApprovalHeaderDto
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public string? TemplateName { get; set; }
    }
}
