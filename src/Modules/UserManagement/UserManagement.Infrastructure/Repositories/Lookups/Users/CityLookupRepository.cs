using System.Data;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Dapper;

namespace UserManagement.Infrastructure.Repositories.Lookups.Users
{
    internal sealed class CityLookupRepository : ICityLookup
    {
        private readonly IDbConnection _dbConnection;

        public CityLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<CityLookupDto>> GetAllCityAsync(CancellationToken ct = default)
        {
            const string sql = @"
                SELECT
                    C.Id AS CityId,
                    C.CityName
                FROM [AppData].[City] C
                WHERE C.IsDeleted = 0
                AND C.IsActive = 1
                ORDER BY C.Id DESC;";

            var cmd = new CommandDefinition(sql, cancellationToken: ct);
            var rows = await _dbConnection.QueryAsync<CityLookupDto>(cmd);
            return rows.ToList();
        }

        public async Task<CityLookupDto?> GetByIdAsync(int cityId, CancellationToken ct = default)
        {
            const string sql = @"
                    SELECT
                        C.Id AS CityId,
                        C.CityName
                    FROM [AppData].[City] C
                    WHERE C.Id = @CityId
                    AND C.IsDeleted = 0
                    AND C.IsActive = 1;
                    ";

            var cmd = new CommandDefinition(sql, new { CityId = cityId }, cancellationToken: ct);
            return await _dbConnection.QueryFirstOrDefaultAsync<CityLookupDto>(cmd);
        }

        public async Task<IReadOnlyList<CityLookupDto>> GetByIdsAsync(IEnumerable<int> cityIds, CancellationToken ct = default)
        {
            var ids = (cityIds ?? Array.Empty<int>())
                .Where(x => x > 0)
                .Distinct()
                .ToArray();

            if (ids.Length == 0)
                return Array.Empty<CityLookupDto>();

            const string sql = @"
                SELECT
                    C.Id AS CityId,
                    C.CityName
                FROM [AppData].[City] C
                WHERE C.Id IN @Ids
                AND C.IsDeleted = 0
                AND C.IsActive = 1;
                ";

            var cmd = new CommandDefinition(sql, new { Ids = ids }, cancellationToken: ct);
            var rows = await _dbConnection.QueryAsync<CityLookupDto>(cmd);

            return rows.ToList();
        }
    }
}
