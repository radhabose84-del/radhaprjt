using System.Data;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Dapper;

namespace UserManagement.Infrastructure.Repositories.Lookups.Users;

internal sealed class DivisionLookupRepository : IDivisionLookup
{
    private readonly IDbConnection _dbConnection;

    public DivisionLookupRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<IReadOnlyList<DivisionLookupDto>> GetAllDivisionAsync()
    {
        const string sql = @"
            SELECT Id, ShortName, Name
            FROM AppData.Division
            WHERE IsActive = 1 AND IsDeleted = 0
            ORDER BY Name ASC;";

        var result = await _dbConnection.QueryAsync<DivisionLookupDto>(sql);
        return result.ToList();
    }

    public async Task<IReadOnlyList<DivisionLookupDto>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var idList = ids?.ToList();
        if (idList == null || idList.Count == 0)
            return new List<DivisionLookupDto>();

        const string sql = @"
            SELECT Id, ShortName, Name
            FROM AppData.Division
            WHERE Id IN @Ids AND IsDeleted = 0
            ORDER BY Name ASC;";

        var result = await _dbConnection.QueryAsync<DivisionLookupDto>(sql, new { Ids = idList });
        return result.ToList();
    }
}
