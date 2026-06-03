using System.Data;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Dapper;

namespace UserManagement.Infrastructure.Repositories.Lookups.Users
{
    internal sealed class LocationMasterLookupRepository : ILocationMasterLookup
    {
        private readonly IDbConnection _dbConnection;

        public LocationMasterLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<LocationMasterLookupDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT L.Id, L.Code, L.LocationName
                FROM [AppData].[Location] L
                WHERE L.Id = @Id AND L.IsActive = 1 AND L.IsDeleted = 0;";

            var cmd = new CommandDefinition(sql, new { Id = id }, cancellationToken: ct);
            return await _dbConnection.QueryFirstOrDefaultAsync<LocationMasterLookupDto>(cmd);
        }

        public async Task<IReadOnlyList<LocationMasterLookupDto>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            var idList = (ids ?? Array.Empty<int>()).Where(x => x > 0).Distinct().ToArray();
            if (idList.Length == 0)
                return Array.Empty<LocationMasterLookupDto>();

            const string sql = @"
                SELECT L.Id, L.Code, L.LocationName
                FROM [AppData].[Location] L
                WHERE L.Id IN @Ids AND L.IsDeleted = 0;";

            var cmd = new CommandDefinition(sql, new { Ids = idList }, cancellationToken: ct);
            var rows = await _dbConnection.QueryAsync<LocationMasterLookupDto>(cmd);
            return rows.ToList();
        }
    }
}
