using System.Data;
using Contracts.Dtos.Lookups.Sales;
using Contracts.Interfaces.Lookups.Sales;
using Dapper;

namespace SalesManagement.Infrastructure.Repositories.Lookups.Sales;

internal sealed class CommissionSplitLookupRepository : ICommissionSplitLookup
{
    private readonly IDbConnection _dbConnection;

    public CommissionSplitLookupRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<IReadOnlyList<CommissionSplitLookupDto>> GetAllCommissionSplitAsync()
    {
        const string sql = @"
            SELECT Id, SplitCode, SplitName
            FROM Sales.CommissionSplit
            WHERE IsActive = 1 AND IsDeleted = 0
            ORDER BY SplitName ASC;
        ";

        var result = await _dbConnection.QueryAsync<CommissionSplitLookupDto>(sql);
        return result.ToList();
    }
}
