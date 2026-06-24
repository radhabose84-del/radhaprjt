namespace FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate
{
    public interface IRecurringJournalTemplateCommandRepository
    {
        // Create template header + lines in one save.
        Task<int> CreateAsync(FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader entity);

        // Update the header + replace its lines (does not affect already-generated journals).
        Task<int> UpdateAsync(FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader entity);

        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
