using System.Data;
using Contracts.Interfaces.Lookups.FixedAssetManagement;
using Dapper;

namespace FAM.Infrastructure.Repositories.Lookups.FixedAssetManagement
{
    internal class CityValidationLookupRepository : ICityValidationLookup
    {
        private readonly IDbConnection _dbConnection;

        public CityValidationLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<bool> IsCityUsedAsync(int cityId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT CASE
                    WHEN EXISTS (
                        SELECT 1 FROM FixedAsset.Manufacture WHERE CityId = @CityId AND IsDeleted = 0
                    ) OR EXISTS (
                        SELECT 1 FROM FixedAsset.AssetWarranty WHERE CityId = @CityId AND IsDeleted = 0
                    )
                    THEN 1
                    ELSE 0
                END;";

            var result = await _dbConnection.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, new { CityId = cityId }, cancellationToken: ct));

            return result == 1;
        }
    }
}
