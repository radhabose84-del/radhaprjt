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
        Task<bool> ItemExistsAsync(int itemId, CancellationToken ct = default);
        Task<bool> CommissionTypeExistsAsync(int commissionTypeId);
        Task<bool> UomExistsAsync(int uomId, CancellationToken ct = default);
        Task<bool> CurrencyExistsAsync(int currencyId, CancellationToken ct = default);
        Task<bool> OverlapExistsAsync(int agentId, int salesSegmentId, int itemId,
            DateTimeOffset validityFrom, DateTimeOffset validityTo, int? excludeId = null);
    }
}
