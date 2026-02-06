using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Contracts.Dtos.Lookups.FixedAssetManagement;
using Contracts.Interfaces.Lookups.FixedAssetManagement;

namespace FAM.Infrastructure.Repositories.Lookups.FixedAssetManagement
{
    internal class AssetSpecificationLookupRepository : IAssetSpecificationLookup
    {
        private readonly IDbConnection _dbConnection;

        public AssetSpecificationLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<AssetSpecificationLookupDto>> GetAllAssetSpecificationAsync()
        {
            const string sql = @"
                SELECT
                    B.AssetId,
                    A.SpecificationName,
                    B.SpecificationValue,
                    C.CapitalizationDate
                FROM FixedAsset.SpecificationMaster A
                INNER JOIN FixedAsset.AssetSpecifications B ON A.Id = B.SpecificationId
                LEFT JOIN FixedAsset.AssetPurchaseDetails C ON B.AssetId = C.AssetId
                WHERE B.IsDeleted = 0
                ORDER BY B.AssetId DESC;
            ";

            var result = await _dbConnection.QueryAsync<AssetSpecificationLookupDto>(sql);
            return result.ToList();
        }

        public async Task<List<AssetSpecificationLookupDto>> GetByAssetIdAsync(int assetId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT
                    B.AssetId,
                    A.SpecificationName,
                    B.SpecificationValue,
                    C.CapitalizationDate
                FROM FixedAsset.SpecificationMaster A
                INNER JOIN FixedAsset.AssetSpecifications B ON A.Id = B.SpecificationId
                LEFT JOIN FixedAsset.AssetPurchaseDetails C ON B.AssetId = C.AssetId
                WHERE B.IsDeleted = 0 AND B.AssetId = @AssetId
                ORDER BY B.AssetId DESC;
            ";

            var result = await _dbConnection.QueryAsync<AssetSpecificationLookupDto>(
                new CommandDefinition(sql, new { AssetId = assetId }, cancellationToken: ct));
            return result.ToList();
        }
    }
}
