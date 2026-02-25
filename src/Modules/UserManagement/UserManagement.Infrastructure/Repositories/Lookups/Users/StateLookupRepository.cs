using System.Data;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Dapper;

namespace UserManagement.Infrastructure.Repositories.Lookups.Users
{
    internal sealed class StateLookupRepository : IStateLookup
    {
        private readonly IDbConnection _dbConnection;

        public StateLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<StateLookupDto>> GetAllStatesAsync(CancellationToken ct = default)
        {
            const string sql = @"
                SELECT
                    S.Id AS StateId,
                    S.StateName
                FROM [AppData].[State] S
                WHERE S.IsDeleted = 0
                AND S.IsActive = 1
                ORDER BY S.Id DESC;
                ";

            var cmd = new CommandDefinition(sql, cancellationToken: ct);
            var rows = await _dbConnection.QueryAsync<StateLookupDto>(cmd);

            return rows.ToList();
        }

        public async Task<StateLookupDto?> GetByIdAsync(int stateId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT
                    S.Id AS StateId,
                    S.StateName
                FROM [AppData].[State] S
                WHERE S.Id = @StateId
                AND S.IsDeleted = 0
                AND S.IsActive = 1;
                ";

            var cmd = new CommandDefinition(sql, new { StateId = stateId }, cancellationToken: ct);
            return await _dbConnection.QueryFirstOrDefaultAsync<StateLookupDto>(cmd);
        }

        public async Task<IReadOnlyList<StateLookupDto>> GetByIdsAsync(IEnumerable<int> stateIds, CancellationToken ct = default)
        {
            var ids = (stateIds ?? Array.Empty<int>())
                .Where(x => x > 0)
                .Distinct()
                .ToArray();

            if (ids.Length == 0)
                return Array.Empty<StateLookupDto>();

            const string sql = @"
                SELECT
                    S.Id AS StateId,
                    S.StateName
                FROM [AppData].[State] S
                WHERE S.Id IN @Ids
                AND S.IsDeleted = 0
                AND S.IsActive = 1;
                ";

            var cmd = new CommandDefinition(sql, new { Ids = ids }, cancellationToken: ct);
            var rows = await _dbConnection.QueryAsync<StateLookupDto>(cmd);

            return rows.ToList();
        }
    }
}
