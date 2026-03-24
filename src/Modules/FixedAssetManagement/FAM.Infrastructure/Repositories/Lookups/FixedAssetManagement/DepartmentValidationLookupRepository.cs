using System.Data;
using Contracts.Interfaces.Lookups.FixedAssetManagement;
using Dapper;

namespace FAM.Infrastructure.Repositories.Lookups.FixedAssetManagement
{
    internal class DepartmentValidationLookupRepository : IDepartmentValidationLookup
    {
        private readonly IDbConnection _dbConnection;

        public DepartmentValidationLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<bool> IsDepartmentUsedAsync(int departmentId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT CASE
                    WHEN EXISTS (
                        SELECT 1 FROM FixedAsset.Location WHERE DepartmentId = @DepartmentId AND IsDeleted = 0
                    ) OR EXISTS (
                        SELECT 1 FROM FixedAsset.SubLocation WHERE DepartmentId = @DepartmentId AND IsDeleted = 0
                    ) OR EXISTS (
                        SELECT 1 FROM FixedAsset.AssetLocation WHERE DepartmentId = @DepartmentId
                    ) OR EXISTS (
                        SELECT 1 FROM FixedAsset.AssetTransfer WHERE DepartmentId = @DepartmentId AND IsDeleted = 0
                    ) OR EXISTS (
                        SELECT 1 FROM FixedAsset.AssetTransferIssueHdr WHERE DepartmentId = @DepartmentId
                    )
                    THEN 1
                    ELSE 0
                END;";

            var result = await _dbConnection.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, new { DepartmentId = departmentId }, cancellationToken: ct));

            return result == 1;
        }
    }
}
