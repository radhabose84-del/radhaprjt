using System.Data;
using Contracts.Dtos.Lookups.Sales;
using Contracts.Interfaces.Lookups.Sales;
using Dapper;

namespace SalesManagement.Infrastructure.Repositories.Lookups.Sales;

internal sealed class SalesGroupLookupRepository : ISalesGroupLookup
{
    private readonly IDbConnection _dbConnection;

    public SalesGroupLookupRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<IReadOnlyList<SalesGroupLookupDto>> GetAllSalesGroupAsync()
    {
        const string sql = @"
            SELECT Id, SalesGroupName
            FROM Sales.SalesGroup
            WHERE IsActive = 1 AND IsDeleted = 0
            ORDER BY SalesGroupName ASC;
        ";

        var result = await _dbConnection.QueryAsync<SalesGroupLookupDto>(sql);
        return result.ToList();
    }
}
