using System.Data;
using Contracts.Dtos.Lookups.Production;
using Contracts.Interfaces.Lookups.Production;
using Dapper;

namespace ProductionManagement.Infrastructure.Repositories.Lookups.Production
{
    internal sealed class YarnTwistMasterLookupRepository : IYarnTwistMasterLookup
    {
        private readonly IDbConnection _dbConnection;

        public YarnTwistMasterLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IReadOnlyList<YarnTwistMasterLookupDto>> GetAllYarnTwistMasterAsync()
        {
            const string sql = @"
                SELECT Id, TwistName
                FROM Production.YarnTwistMaster
                WHERE IsActive = 1 AND IsDeleted = 0
                ORDER BY TwistName ASC;";

            var result = await _dbConnection.QueryAsync<YarnTwistMasterLookupDto>(sql);
            return result.ToList();
        }

        public async Task<IReadOnlyList<YarnTwistMasterLookupDto>> GetByIdsAsync(IEnumerable<int> ids)
        {
            var idList = ids?.Distinct().ToList();
            if (idList == null || idList.Count == 0)
                return new List<YarnTwistMasterLookupDto>();

            const string sql = @"
                SELECT Id, TwistName
                FROM Production.YarnTwistMaster
                WHERE Id IN @Ids AND IsDeleted = 0;";

            var result = await _dbConnection.QueryAsync<YarnTwistMasterLookupDto>(sql, new { Ids = idList });
            return result.ToList();
        }
    }
}
