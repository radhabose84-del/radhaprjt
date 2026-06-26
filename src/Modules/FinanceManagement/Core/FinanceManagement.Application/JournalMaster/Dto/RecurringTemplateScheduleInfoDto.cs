namespace FinanceManagement.Application.JournalMaster.Dto
{
    // Inputs the Hangfire scheduler needs to decide whether (and how) to schedule a template's auto-post job.
    public sealed class RecurringTemplateScheduleInfoDto
    {
        public int Id { get; set; }
        public string? TemplateName { get; set; }
        public int CompanyId { get; set; }
        public string? FrequencyCode { get; set; }     // RECURRING_FREQUENCY code (e.g. MONTHLY / QUARTERLY / ANNUALLY)
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool AutoPost { get; set; }
        public bool LowRisk { get; set; }
        public string? StatusCode { get; set; }        // ApprovalStatus code (Pending / Approved / Rejected)
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }             // soft-deleted templates are never schedulable
    }
}
