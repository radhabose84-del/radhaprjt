using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.FixedAssetManagement;
using Contracts.Interfaces.Lookups.FixedAssetManagement;
using Dapper;

namespace FAM.Infrastructure.Repositories.Lookups.FixedAssetManagement
{
    internal class AssetGroupLookupRepository : IAssetGroupLookup
    {
        private readonly IDbConnection _dbConnection;

        public AssetGroupLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IReadOnlyList<AssetGroupLookupDto>> GetByIdsAsync(
            IEnumerable<int> assetGroupIds,
            CancellationToken ct = default)
        {
            var ids = (assetGroupIds ?? Enumerable.Empty<int>())
                .Where(x => x > 0)
                .Distinct()
                .ToArray();

            if (ids.Length == 0)
                return new List<AssetGroupLookupDto>();

            const string sql = @"
                SELECT
                    Id AS AssetGroupId,
                    Code,
                    GroupName
                FROM FixedAsset.AssetGroup
                WHERE IsDeleted = 0
                  AND IsActive = 1
                  AND Id IN @Ids;";

            var result = await _dbConnection.QueryAsync<AssetGroupLookupDto>(
                new CommandDefinition(sql, new { Ids = ids }, cancellationToken: ct));

            return result.ToList();
        }
    }
}
