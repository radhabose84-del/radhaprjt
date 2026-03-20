using System.Data;
using Contracts.Dtos.Lookups.Production;
using Contracts.Interfaces.Lookups.Production;
using Dapper;

namespace ProductionManagement.Infrastructure.Repositories.Lookups.Production
{
    internal sealed class PackTypeLookupRepository : IPackTypeLookup
    {
        private readonly IDbConnection _dbConnection;

        public PackTypeLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IReadOnlyList<PackTypeLookupDto>> GetAllAsync(CancellationToken ct = default)
        {
            const string sql = @"
                SELECT Id, PackTypeCode, PackTypeName, NetWeight, TareWeight, GrossWeight, ConesPerBag
                FROM Production.PackType
                WHERE IsActive = 1 AND IsDeleted = 0
                ORDER BY PackTypeName ASC;";

            var result = await _dbConnection.QueryAsync<PackTypeLookupDto>(sql);
            return result.ToList();
        }

        public async Task<IReadOnlyList<PackTypeLookupDto>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            var idList = ids?.Distinct().ToList();
            if (idList == null || idList.Count == 0)
                return new List<PackTypeLookupDto>();

            const string sql = @"
                SELECT Id, PackTypeCode, PackTypeName, NetWeight, TareWeight, GrossWeight, ConesPerBag
                FROM Production.PackType
                WHERE Id IN @Ids AND IsDeleted = 0;";

            var result = await _dbConnection.QueryAsync<PackTypeLookupDto>(sql, new { Ids = idList });
            return result.ToList();
        }
    }
}
