using System.Data;
using Dapper;
using SalesManagement.Application.Common.Interfaces.IMovementTypeConfig;
using SalesManagement.Application.MovementTypeConfig.Dto;

namespace SalesManagement.Infrastructure.Repositories.MovementTypeConfig
{
    public class MovementTypeConfigQueryRepository : IMovementTypeConfigQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public MovementTypeConfigQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<MovementTypeConfigDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.MovementTypeConfig mtc
                WHERE mtc.IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (mtc.MovementCode LIKE @Search OR mtc.MovementDescription LIKE @Search)")}};

                SELECT
                    mtc.Id, mtc.MovementCode, mtc.MovementDescription,
                    mtc.MovementCategoryId,  mc.Description  AS MovementCategoryName,
                    mtc.FromStockTypeId,     fst.Description AS FromStockTypeName,
                    mtc.ToStockTypeId,       tst.Description AS ToStockTypeName,
                    mtc.QuantityUpdateFlag, mtc.ValueUpdateFlag, mtc.AccountModifier,
                    mtc.BatchRequiredFlag, mtc.NegativeStockAllowed,
                    mtc.IsActive, mtc.IsDeleted,
                    mtc.CreatedBy, mtc.CreatedDate, mtc.CreatedByName, mtc.CreatedIP,
                    mtc.ModifiedBy, mtc.ModifiedDate, mtc.ModifiedByName, mtc.ModifiedIP
                FROM Sales.MovementTypeConfig mtc
                LEFT JOIN Sales.MiscMaster mc  ON mtc.MovementCategoryId = mc.Id  AND mc.IsDeleted  = 0
                LEFT JOIN Sales.MiscMaster fst ON mtc.FromStockTypeId    = fst.Id AND fst.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster tst ON mtc.ToStockTypeId      = tst.Id AND tst.IsDeleted = 0
                WHERE mtc.IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (mtc.MovementCode LIKE @Search OR mtc.MovementDescription LIKE @Search)")}}
                ORDER BY mtc.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new { Search = $"%{searchTerm}%", Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };
            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<MovementTypeConfigDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            return (list, totalCount);
        }

        public async Task<MovementTypeConfigDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    mtc.Id, mtc.MovementCode, mtc.MovementDescription,
                    mtc.MovementCategoryId,  mc.Description  AS MovementCategoryName,
                    mtc.FromStockTypeId,     fst.Description AS FromStockTypeName,
                    mtc.ToStockTypeId,       tst.Description AS ToStockTypeName,
                    mtc.QuantityUpdateFlag, mtc.ValueUpdateFlag, mtc.AccountModifier,
                    mtc.BatchRequiredFlag, mtc.NegativeStockAllowed,
                    mtc.IsActive, mtc.IsDeleted,
                    mtc.CreatedBy, mtc.CreatedDate, mtc.CreatedByName, mtc.CreatedIP,
                    mtc.ModifiedBy, mtc.ModifiedDate, mtc.ModifiedByName, mtc.ModifiedIP
                FROM Sales.MovementTypeConfig mtc
                LEFT JOIN Sales.MiscMaster mc  ON mtc.MovementCategoryId = mc.Id  AND mc.IsDeleted  = 0
                LEFT JOIN Sales.MiscMaster fst ON mtc.FromStockTypeId    = fst.Id AND fst.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster tst ON mtc.ToStockTypeId      = tst.Id AND tst.IsDeleted = 0
                WHERE mtc.Id = @Id AND mtc.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<MovementTypeConfigDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<MovementTypeConfigLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 20 Id, MovementCode, MovementDescription
                FROM Sales.MovementTypeConfig
                WHERE IsDeleted = 0 AND IsActive = 1
                AND (MovementCode LIKE @Term OR MovementDescription LIKE @Term)
                ORDER BY MovementDescription ASC";

            var result = await _dbConnection.QueryAsync<MovementTypeConfigLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string movementCode, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM Sales.MovementTypeConfig
                WHERE MovementCode = @Code
                AND IsDeleted = 0";

            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Code = movementCode.Trim(), Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.MovementTypeConfig
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> MiscMasterExistsAsync(int miscMasterId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.MiscMaster
                WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = miscMasterId });
            return count > 0;
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [Sales].[StoTypeMaster] WHERE PgiMovementTypeId = @id AND IsDeleted = 0
                    UNION ALL
                    SELECT 1 FROM [Sales].[StoTypeMaster] WHERE GrMovementTypeId = @id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { id });
        }

        public async Task<bool> IsMovementTypeConfigLinkedAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [Sales].[StoTypeMaster] WHERE PgiMovementTypeId = @id AND IsDeleted = 0 AND IsActive = 1
                    UNION ALL
                    SELECT 1 FROM [Sales].[StoTypeMaster] WHERE GrMovementTypeId = @id AND IsDeleted = 0 AND IsActive = 1
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { id });
        }
    }
}
