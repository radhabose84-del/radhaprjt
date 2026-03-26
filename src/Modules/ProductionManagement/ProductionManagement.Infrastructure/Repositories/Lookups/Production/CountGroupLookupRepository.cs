using System.Data;
using Contracts.Dtos.Lookups.Production;
using Contracts.Interfaces.Lookups.Production;
using Dapper;

namespace ProductionManagement.Infrastructure.Repositories.Lookups.Production
{
    internal sealed class CountGroupLookupRepository : ICountGroupLookup
    {
        private readonly IDbConnection _dbConnection;

        public CountGroupLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IReadOnlyList<CountGroupLookupDto>> GetAllAsync(CancellationToken ct = default)
        {
            const string sql = @"
                SELECT Id, CountGroupCode, CountGroupName
                FROM Production.CountGroup
                WHERE IsActive = 1 AND IsDeleted = 0
                ORDER BY CountGroupName ASC;";

            var result = await _dbConnection.QueryAsync<CountGroupLookupDto>(sql);
            return result.ToList();
        }

        public async Task<IReadOnlyList<CountGroupLookupDto>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            var idList = ids?.Distinct().ToList();
            if (idList == null || idList.Count == 0)
                return new List<CountGroupLookupDto>();

            const string sql = @"
                SELECT Id, CountGroupCode, CountGroupName
                FROM Production.CountGroup
                WHERE Id IN @Ids AND IsDeleted = 0;";

            var result = await _dbConnection.QueryAsync<CountGroupLookupDto>(sql, new { Ids = idList });
            return result.ToList();
        }
    }
}
