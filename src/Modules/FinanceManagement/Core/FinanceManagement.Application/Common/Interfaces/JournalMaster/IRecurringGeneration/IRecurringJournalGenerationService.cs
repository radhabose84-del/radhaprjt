namespace FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringGeneration
{
    // US-GL01-11B — generates the journal for ONE recurring template. Idempotent per (company, template, period).
    public interface IRecurringJournalGenerationService
    {
        // Generate ONE template's JV for the given period (idempotent per company+template+period).
        // NEVER auto-posts — status follows the template's LowRisk flag: LowRisk → APPROVED (posted manually later);
        // high-risk → DRAFT + approval workflow. Currency/exchange-rate come from each detail line.
        // Returns the generated journal id, or 0 if the period isn't open / already generated.
        Task<int> GenerateForTemplateAsync(int companyId, int templateId, DateOnly voucherDate, CancellationToken ct);
    }
}
