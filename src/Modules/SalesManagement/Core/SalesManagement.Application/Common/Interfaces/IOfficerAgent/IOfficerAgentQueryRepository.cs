using SalesManagement.Application.OfficerAgent.Dto;

namespace SalesManagement.Application.Common.Interfaces.IOfficerAgent
{
    public interface IOfficerAgentQueryRepository
    {
        Task<(List<OfficerAgentGroupedDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<OfficerAgentGroupedDto?> GetByIdAsync(int marketingOfficerId);
        Task<IReadOnlyList<OfficerAgentGroupedDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> NotFoundAsync(int id);
        Task<bool> AgentExistsAsync(int agentId, CancellationToken ct = default);
        Task<bool> MarketingOfficerExistsAsync(int officerId);
        Task<bool> IsExpiredAsync(int id);
        Task<bool> AlreadyAssignedAsync(int marketingOfficerId, int agentId, int? excludeAssignmentId = null);
        Task<bool> IsExpiredAndModifiedAsync(int id, int agentId, DateOnly validityFrom, DateOnly validityTo, int isActive);
    }
}
