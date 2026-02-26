namespace SalesManagement.Application.Common.Interfaces.IAgentCommissionConfig
{
    public interface IAgentCommissionConfigCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.AgentCommissionConfig entity);
        Task<int> UpdateAsync(Domain.Entities.AgentCommissionConfig entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
