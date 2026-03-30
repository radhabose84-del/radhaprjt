using System.Data;
using Contracts.Dtos.Lookups.Production;
using Contracts.Interfaces.Lookups.Production;
using Dapper;

namespace ProductionManagement.Infrastructure.Repositories.Lookups.Production
{
    internal sealed class YarnTypeLookupRepository : IYarnTypeLookup
    {
        private readonly IDbConnection _dbConnection;

        public YarnTypeLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IReadOnlyList<YarnTypeLookupDto>> GetAllAsync(CancellationToken ct = default)
        {
            const string sql = @"
                SELECT Id, YarnTypeCode, YarnTypeName
                FROM Production.YarnType
                WHERE IsActive = 1 AND IsDeleted = 0
                ORDER BY YarnTypeName ASC;";

            var result = await _dbConnection.QueryAsync<YarnTypeLookupDto>(sql);
            return result.ToList();
        }

        public async Task<IReadOnlyList<YarnTypeLookupDto>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            var idList = ids?.Distinct().ToList();
            if (idList == null || idList.Count == 0)
                return new List<YarnTypeLookupDto>();

            const string sql = @"
                SELECT Id, YarnTypeCode, YarnTypeName
                FROM Production.YarnType
                WHERE Id IN @Ids AND IsDeleted = 0;";

            var result = await _dbConnection.QueryAsync<YarnTypeLookupDto>(sql, new { Ids = idList });
            return result.ToList();
        }
    }
}
