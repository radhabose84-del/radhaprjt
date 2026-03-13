using SalesManagement.Application.AgentCustomerMapping.Dto;

namespace SalesManagement.Application.Common.Interfaces.IAgentCustomerMapping
{
    public interface IAgentCustomerMappingQueryRepository
    {
        Task<(List<AgentCustomerMappingDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<AgentCustomerMappingDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<AgentCustomerMappingLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> NotFoundAsync(int id);
        Task<bool> CustomerExistsAsync(int customerId, CancellationToken ct = default);
        Task<bool> AgentExistsAsync(int agentId, CancellationToken ct = default);
        Task<bool> SalesSegmentExistsAsync(int salesSegmentId, CancellationToken ct = default);
        Task<bool> SoftDeleteValidationAsync(int id, CancellationToken ct = default);
    }
}
