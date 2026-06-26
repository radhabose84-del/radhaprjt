namespace FinanceManagement.Application.JournalMaster.Dto
{
    // One row in the "Generated Instances" grid — a journal created from a recurring template
    // (RecurringGenerationLog joined to its generated JournalHeader + template).
    public sealed class RecurringGeneratedInstanceDto
    {
        public int Id { get; set; }                     // RecurringGenerationLog id
        public int? GeneratedVoucherId { get; set; }    // JournalHeader id (null only if mid-generation)
        public string? VoucherNo { get; set; }          // e.g. JV/2026-27/0388
        public int TemplateId { get; set; }
        public string? TemplateName { get; set; }
        public string? Period { get; set; }             // accounting period id (idempotency key)
        public string? PeriodName { get; set; }          // friendly period label for display (from AccountingPeriod)
        public decimal Amount { get; set; }             // journal TotalDr
        public DateTimeOffset GeneratedAt { get; set; }
        public bool AutoPosted { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }         // journal status (Draft / Posted / Pending Approval …)
    }
}
