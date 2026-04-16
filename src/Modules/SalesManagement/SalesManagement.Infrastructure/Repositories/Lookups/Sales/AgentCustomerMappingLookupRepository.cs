using System.Data;
using Contracts.Interfaces.Lookups.Sales;
using Dapper;

namespace SalesManagement.Infrastructure.Repositories.Lookups.Sales;

internal sealed class AgentCustomerMappingLookupRepository : IAgentCustomerMappingLookup
{
    private readonly IDbConnection _dbConnection;

    public AgentCustomerMappingLookupRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<IReadOnlyList<int>> GetCustomerIdsByAgentAsync(int agentId)
    {
        const string sql = @"
            SELECT DISTINCT CustomerId
            FROM Sales.AgentCustomerMapping
            WHERE AgentId = @AgentId
              AND IsDeleted = 0
              AND IsActive  = 1
              AND EffectiveFrom <= GETDATE()
              AND (EffectiveTo IS NULL OR EffectiveTo >= GETDATE());
        ";

        var result = await _dbConnection.QueryAsync<int>(sql, new { AgentId = agentId });
        return result.ToList();
    }
}
