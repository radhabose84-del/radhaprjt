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
        Task<bool> MiscMasterExistsAsync(int id);
        Task<bool> CommissionSplitExistsAsync(int id);
        Task<bool> SalesGroupExistsAsync(int id);
        Task<bool> PaymentTermExistsAsync(int id, CancellationToken ct = default);
        Task<bool> OverlapExistsAsync(int agentId, int commissionSplitId,
            DateTimeOffset validityFrom, DateTimeOffset? validityTo, int? excludeId = null);
        Task<bool> IsAgentCommissionConfigLinkedAsync(int id);
    }
}
