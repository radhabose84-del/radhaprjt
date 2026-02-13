using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Interfaces.Lookups.FixedAssetManagement;
using Dapper;

namespace FAM.Infrastructure.Repositories.Lookups.FixedAssetManagement
{
    internal class CountryValidationLookupRepository : ICountryValidationLookup
    {
        private readonly IDbConnection _dbConnection;

        public CountryValidationLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<bool> IsCountryUsedAsync(int countryId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT CASE
                    WHEN EXISTS (
                        SELECT 1 FROM FixedAsset.Manufacture WHERE CountryId = @CountryId AND IsDeleted = 0
                    ) OR EXISTS (
                        SELECT 1 FROM FixedAsset.AssetWarranty WHERE CountryId = @CountryId AND IsDeleted = 0
                    )
                    THEN 1
                    ELSE 0
                END;";

            var result = await _dbConnection.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, new { CountryId = countryId }, cancellationToken: ct));

            return result == 1;
        }
    }
}
