using FinanceManagement.Application.JournalMaster.Dto;

namespace FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalThresholdRule
{
    public interface IJournalThresholdRuleQueryRepository
    {
        Task<(List<JournalThresholdRuleDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<JournalThresholdRuleDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<JournalThresholdRuleLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> NotFoundAsync(int id);
        Task<bool> RuleTypeExistsAsync(int ruleTypeId);

        // US-GL01-16B — flag read model (flags are written by the engine, never via this API).
        Task<(List<JournalFlagDto>, int)> GetFlagsAsync(int pageNumber, int pageSize, int? journalHeaderId);
    }
}
