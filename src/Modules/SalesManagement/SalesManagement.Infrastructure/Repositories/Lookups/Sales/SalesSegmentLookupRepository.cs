using System.Data;
using Contracts.Dtos.Lookups.Sales;
using Contracts.Interfaces.Lookups.Sales;
using Dapper;

namespace SalesManagement.Infrastructure.Repositories.Lookups.Sales;

internal sealed class SalesSegmentLookupRepository : ISalesSegmentLookup
{
    private readonly IDbConnection _dbConnection;

    public SalesSegmentLookupRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<IReadOnlyList<SalesSegmentLookupDto>> GetAllSalesSegmentAsync()
    {
        const string sql = @"
            SELECT Id, SegmentName
            FROM Sales.SalesSegment
            WHERE IsActive = 1 AND IsDeleted = 0
            ORDER BY SegmentName ASC;
        ";

        var result = await _dbConnection.QueryAsync<SalesSegmentLookupDto>(sql);
        return result.ToList();
    }
}
