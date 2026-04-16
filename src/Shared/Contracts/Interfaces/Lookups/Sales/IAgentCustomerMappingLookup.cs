namespace Contracts.Interfaces.Lookups.Sales;

public interface IAgentCustomerMappingLookup
{
    Task<IReadOnlyList<int>> GetCustomerIdsByAgentAsync(int agentId);
}
