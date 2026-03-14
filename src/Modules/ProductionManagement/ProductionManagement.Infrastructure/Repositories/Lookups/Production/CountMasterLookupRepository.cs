using System.Data;
using Contracts.Dtos.Lookups.Production;
using Contracts.Interfaces.Lookups.Production;
using Dapper;

namespace ProductionManagement.Infrastructure.Repositories.Lookups.Production
{
    internal sealed class CountMasterLookupRepository : ICountMasterLookup
    {
        private readonly IDbConnection _dbConnection;

        public CountMasterLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IReadOnlyList<CountMasterLookupDto>> GetAllAsync(CancellationToken ct = default)
        {
            const string sql = @"
                SELECT Id, CountCode, CountDescription
                FROM Production.CountMaster
                WHERE IsActive = 1 AND IsDeleted = 0
                ORDER BY CountDescription ASC;";

            var result = await _dbConnection.QueryAsync<CountMasterLookupDto>(sql);
            return result.ToList();
        }

        public async Task<IReadOnlyList<CountMasterLookupDto>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            var idList = ids?.Distinct().ToList();
            if (idList == null || idList.Count == 0)
                return new List<CountMasterLookupDto>();

            const string sql = @"
                SELECT Id, CountCode, CountDescription
                FROM Production.CountMaster
                WHERE Id IN @Ids AND IsDeleted = 0;";

            var result = await _dbConnection.QueryAsync<CountMasterLookupDto>(sql, new { Ids = idList });
            return result.ToList();
        }
    }
}
