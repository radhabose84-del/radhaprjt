using System.Data;
using Contracts.Dtos.Lookups.Production;
using Contracts.Interfaces.Lookups.Production;
using Dapper;

namespace ProductionManagement.Infrastructure.Repositories.Lookups.Production
{
    internal sealed class LotMasterLookupRepository : ILotMasterLookup
    {
        private readonly IDbConnection _dbConnection;

        public LotMasterLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IReadOnlyList<LotMasterLookupDto>> GetAllAsync(CancellationToken ct = default)
        {
            const string sql = @"
                SELECT Id, LotCode, BatchNumber, ItemId
                FROM Production.LotMaster
                WHERE IsActive = 1 AND IsDeleted = 0
                ORDER BY LotCode ASC;";

            var result = await _dbConnection.QueryAsync<LotMasterLookupDto>(sql);
            return result.ToList();
        }

        public async Task<IReadOnlyList<LotMasterLookupDto>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            var idList = ids?.Distinct().ToList();
            if (idList == null || idList.Count == 0)
                return new List<LotMasterLookupDto>();

            const string sql = @"
                SELECT Id, LotCode, BatchNumber, ItemId
                FROM Production.LotMaster
                WHERE Id IN @Ids AND IsDeleted = 0;";

            var result = await _dbConnection.QueryAsync<LotMasterLookupDto>(sql, new { Ids = idList });
            return result.ToList();
        }
    }
}
