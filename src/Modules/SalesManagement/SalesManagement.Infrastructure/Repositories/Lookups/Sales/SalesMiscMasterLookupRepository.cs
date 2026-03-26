using System.Data;
using Contracts.Dtos.Lookups.Sales;
using Contracts.Interfaces.Lookups.Sales;
using Dapper;

namespace SalesManagement.Infrastructure.Repositories.Lookups.Sales;

internal sealed class SalesMiscMasterLookupRepository : ISalesMiscMasterLookup
{
    private readonly IDbConnection _dbConnection;

    public SalesMiscMasterLookupRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<SalesMiscMasterLookupDto?> GetByCodeAsync(string code)
    {
        const string sql = @"
            SELECT Id, Code, Description
            FROM Sales.MiscMaster
            WHERE IsDeleted = 0 AND IsActive = 1
              AND LOWER(Code) = LOWER(@Code);";

        return await _dbConnection.QueryFirstOrDefaultAsync<SalesMiscMasterLookupDto>(sql, new { Code = code });
    }
}
