using SalesManagement.Application.AgentCommissionConfig.Dto;

namespace SalesManagement.Application.Common.Interfaces.IAgentCommissionConfig
{
    public interface IAgentCommissionConfigQueryRepository
    {
        Task<(List<AgentCommissionConfigDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<AgentCommissionConfigDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<AgentCommissionConfigLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> NotFoundAsync(int id);
        Task<bool> AgentExistsAsync(int agentId, CancellationToken ct = default);
        Task<bool> SalesSegmentExistsAsync(int salesSegmentId);
        Task<bool> CommissionTypeExistsAsync(int commissionTypeId);
        Task<bool> CommissionBasisExistsAsync(int commissionBasisId);
        Task<bool> ApplicableLevelExistsAsync(int applicableLevelId);
        Task<bool> CurrencyExistsAsync(int currencyId, CancellationToken ct = default);
        Task<bool> OverlapExistsAsync(int agentId, int salesSegmentId,
            DateTimeOffset validityFrom, DateTimeOffset validityTo, int? excludeId = null);
    }
}
