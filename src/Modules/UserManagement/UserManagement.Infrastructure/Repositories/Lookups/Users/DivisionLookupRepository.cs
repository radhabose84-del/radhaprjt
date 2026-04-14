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

    public async Task<List<DivisionUnitLookupDto>> GetUnitsByDivisionAsync(int companyId, int divisionId, CancellationToken ct = default)
    {
        if (companyId <= 0 || divisionId <= 0)
            return new List<DivisionUnitLookupDto>();

        const string sql = @"
            SELECT
                U.Id AS UnitId,
                U.UnitName,
                D.Id AS DivisionId,
                D.Name AS DivisionName,
                C.Id AS CompanyId,
                C.CompanyName
            FROM [AppData].[Unit] U
            INNER JOIN [AppData].[Division] D ON D.Id = U.DivisionId
            INNER JOIN [AppData].[Company] C ON C.Id = U.CompanyId
            WHERE U.IsDeleted = 0
              AND D.IsDeleted = 0
              AND C.IsDeleted = 0
              AND U.CompanyId = @CompanyId
              AND U.DivisionId = @DivisionId
            ORDER BY U.UnitName ASC;";

        var cmd = new CommandDefinition(
            sql,
            new { CompanyId = companyId, DivisionId = divisionId },
            cancellationToken: ct);

        var result = await _dbConnection.QueryAsync<DivisionUnitLookupDto>(cmd);
        return result.ToList();
    }
}
