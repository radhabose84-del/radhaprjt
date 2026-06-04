using System.Data;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Dapper;

namespace UserManagement.Infrastructure.Repositories.Lookups.Users
{
    internal sealed class StationLookupRepository : IStationLookup
    {
        private readonly IDbConnection _dbConnection;

        public StationLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<StationLookupDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT S.Id, S.Code, S.StationName
                FROM [AppData].[Station] S
                WHERE S.Id = @Id AND S.IsActive = 1 AND S.IsDeleted = 0;";

            var cmd = new CommandDefinition(sql, new { Id = id }, cancellationToken: ct);
            return await _dbConnection.QueryFirstOrDefaultAsync<StationLookupDto>(cmd);
        }

        public async Task<IReadOnlyList<StationLookupDto>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            var idList = (ids ?? Array.Empty<int>()).Where(x => x > 0).Distinct().ToArray();
            if (idList.Length == 0)
                return Array.Empty<StationLookupDto>();

            const string sql = @"
                SELECT S.Id, S.Code, S.StationName
                FROM [AppData].[Station] S
                WHERE S.Id IN @Ids AND S.IsDeleted = 0;";

            var cmd = new CommandDefinition(sql, new { Ids = idList }, cancellationToken: ct);
            var rows = await _dbConnection.QueryAsync<StationLookupDto>(cmd);
            return rows.ToList();
        }
    }
}
