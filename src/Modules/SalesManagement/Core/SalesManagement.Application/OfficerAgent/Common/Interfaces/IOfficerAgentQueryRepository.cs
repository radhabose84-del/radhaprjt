using SalesManagement.Application.OfficerAgent.Dto;

namespace SalesManagement.Application.Common.Interfaces.IOfficerAgent
{
    public interface IOfficerAgentQueryRepository
    {
        Task<(List<OfficerAgentDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<OfficerAgentDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<OfficerAgentLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> NotFoundAsync(int id);
        Task<bool> AgentExistsAsync(int agentId, CancellationToken ct = default);
        Task<bool> MarketingOfficerExistsAsync(int officerId);
        Task<bool> IsExpiredAsync(int id);
    }
}
