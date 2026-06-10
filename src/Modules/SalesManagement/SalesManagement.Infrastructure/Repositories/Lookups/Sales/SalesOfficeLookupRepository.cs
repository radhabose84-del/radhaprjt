using System.Data;
using Contracts.Dtos.Lookups.Sales;
using Contracts.Interfaces.Lookups.Sales;
using Dapper;

namespace SalesManagement.Infrastructure.Repositories.Lookups.Sales;

internal sealed class SalesOfficeLookupRepository : ISalesOfficeLookup
{
    private readonly IDbConnection _dbConnection;

    public SalesOfficeLookupRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<IReadOnlyList<SalesOfficeLookupDto>> GetAllSalesOfficeAsync()
    {
        const string sql = @"
            SELECT Id, SalesOfficeName
            FROM Sales.SalesOffice
            WHERE IsActive = 1 AND IsDeleted = 0
            ORDER BY SalesOfficeName ASC;
        ";

        var result = await _dbConnection.QueryAsync<SalesOfficeLookupDto>(sql);
        return result.ToList();
    }
}
