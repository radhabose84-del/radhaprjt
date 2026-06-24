namespace FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalThresholdRule
{
    public interface IJournalThresholdRuleCommandRepository
    {
        Task<int> CreateAsync(FinanceManagement.Domain.Entities.JournalThresholdRule entity);
        Task<int> UpdateAsync(FinanceManagement.Domain.Entities.JournalThresholdRule entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
