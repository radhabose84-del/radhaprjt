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
        Task<bool> SubAgentExistsAsync(int subAgentId, CancellationToken ct = default);
        Task<bool> SalesGroupExistsAsync(int salesGroupId, CancellationToken ct = default);
        Task<bool> SoftDeleteValidationAsync(int id, CancellationToken ct = default);
        Task<List<AgentCustomerMappingDto>> GetByCustomerIdAsync(int customerId, CancellationToken ct = default);
        Task<bool> MappingAlreadyExistsAsync(int customerId, int agentId, CancellationToken ct = default);
        Task<(List<AgentCustomerMappingDto>, int)> GetByFilterAsync(int? salesGroupId, int? customerId, CancellationToken ct = default);
        Task<List<AgentCustomerMappingDto>> GetByMarketingOfficerIdAsync(int marketingOfficerId, CancellationToken ct = default);
    }
}
