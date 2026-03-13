namespace SalesManagement.Application.Common.Interfaces.IAgentCustomerMapping
{
    public interface IAgentCustomerMappingCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.AgentCustomerMapping entity);
        Task<int> UpdateAsync(Domain.Entities.AgentCustomerMapping entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
