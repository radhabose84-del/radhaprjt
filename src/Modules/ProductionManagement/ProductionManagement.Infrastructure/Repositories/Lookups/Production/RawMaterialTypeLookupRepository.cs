using System.Data;
using Contracts.Dtos.Lookups.Production;
using Contracts.Interfaces.Lookups.Production;
using Dapper;

namespace ProductionManagement.Infrastructure.Repositories.Lookups.Production
{
    internal sealed class RawMaterialTypeLookupRepository : IRawMaterialTypeLookup
    {
        private readonly IDbConnection _dbConnection;

        public RawMaterialTypeLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IReadOnlyList<RawMaterialTypeLookupDto>> GetAllAsync(CancellationToken ct = default)
        {
            const string sql = @"
                SELECT Id, RawMaterialTypeCode, RawMaterialTypeName
                FROM Production.RawMaterialType
                WHERE IsActive = 1 AND IsDeleted = 0
                ORDER BY RawMaterialTypeName ASC;";

            var result = await _dbConnection.QueryAsync<RawMaterialTypeLookupDto>(sql);
            return result.ToList();
        }

        public async Task<IReadOnlyList<RawMaterialTypeLookupDto>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            var idList = ids?.Distinct().ToList();
            if (idList == null || idList.Count == 0)
                return new List<RawMaterialTypeLookupDto>();

            const string sql = @"
                SELECT Id, RawMaterialTypeCode, RawMaterialTypeName
                FROM Production.RawMaterialType
                WHERE Id IN @Ids AND IsDeleted = 0;";

            var result = await _dbConnection.QueryAsync<RawMaterialTypeLookupDto>(sql, new { Ids = idList });
            return result.ToList();
        }
    }
}
