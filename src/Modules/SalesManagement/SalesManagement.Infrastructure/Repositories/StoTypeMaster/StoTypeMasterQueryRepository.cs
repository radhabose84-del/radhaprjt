using System.Data;
using Dapper;
using SalesManagement.Application.Common.Interfaces.IStoTypeMaster;
using SalesManagement.Application.StoTypeMaster.Dto;

namespace SalesManagement.Infrastructure.Repositories.StoTypeMaster
{
    public class StoTypeMasterQueryRepository : IStoTypeMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public StoTypeMasterQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<StoTypeMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? string.Empty
                : @" AND (stm.StoTypeCode LIKE @SearchTerm
                       OR stm.StoTypeName LIKE @SearchTerm
                       OR stm.Description LIKE @SearchTerm
                       OR pgi.MovementCode LIKE @SearchTerm
                       OR pgi.MovementDescription LIKE @SearchTerm
                       OR gr.MovementCode LIKE @SearchTerm
                       OR gr.MovementDescription LIKE @SearchTerm)";

            var countSql = $@"
                SELECT COUNT(*)
                FROM Sales.StoTypeMaster stm
                LEFT JOIN Sales.MovementTypeConfig pgi ON stm.PgiMovementTypeId = pgi.Id AND pgi.IsDeleted = 0
                LEFT JOIN Sales.MovementTypeConfig gr ON stm.GrMovementTypeId = gr.Id AND gr.IsDeleted = 0
                WHERE stm.IsDeleted = 0 {searchFilter};";

            var dataSql = $@"
                SELECT
                    stm.Id,
                    stm.StoTypeCode,
                    stm.StoTypeName,
                    stm.Description,
                    stm.PgiMovementTypeId,
                    pgi.MovementCode AS PgiMovementCode,
                    pgi.MovementDescription AS PgiMovementDescription,
                    stm.GrMovementTypeId,
                    gr.MovementCode AS GrMovementCode,
                    gr.MovementDescription AS GrMovementDescription,
                    stm.IsActive,
                    stm.IsDeleted,
                    stm.CreatedBy,
                    stm.CreatedDate,
                    stm.CreatedByName,
                    stm.CreatedIP,
                    stm.ModifiedBy,
                    stm.ModifiedDate,
                    stm.ModifiedByName,
                    stm.ModifiedIP
                FROM Sales.StoTypeMaster stm
                LEFT JOIN Sales.MovementTypeConfig pgi ON stm.PgiMovementTypeId = pgi.Id AND pgi.IsDeleted = 0
                LEFT JOIN Sales.MovementTypeConfig gr ON stm.GrMovementTypeId = gr.Id AND gr.IsDeleted = 0
                WHERE stm.IsDeleted = 0 {searchFilter}
                ORDER BY stm.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            var parameters = new
            {
                SearchTerm = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            using var multi = await _dbConnection.QueryMultipleAsync(
                countSql + dataSql, parameters);

            var totalCount = await multi.ReadFirstAsync<int>();
            var data = (await multi.ReadAsync<StoTypeMasterDto>()).ToList();

            return (data, totalCount);
        }

        public async Task<StoTypeMasterDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    stm.Id,
                    stm.StoTypeCode,
                    stm.StoTypeName,
                    stm.Description,
                    stm.PgiMovementTypeId,
                    pgi.MovementCode AS PgiMovementCode,
                    pgi.MovementDescription AS PgiMovementDescription,
                    stm.GrMovementTypeId,
                    gr.MovementCode AS GrMovementCode,
                    gr.MovementDescription AS GrMovementDescription,
                    stm.IsActive,
                    stm.IsDeleted,
                    stm.CreatedBy,
                    stm.CreatedDate,
                    stm.CreatedByName,
                    stm.CreatedIP,
                    stm.ModifiedBy,
                    stm.ModifiedDate,
                    stm.ModifiedByName,
                    stm.ModifiedIP
                FROM Sales.StoTypeMaster stm
                LEFT JOIN Sales.MovementTypeConfig pgi ON stm.PgiMovementTypeId = pgi.Id AND pgi.IsDeleted = 0
                LEFT JOIN Sales.MovementTypeConfig gr ON stm.GrMovementTypeId = gr.Id AND gr.IsDeleted = 0
                WHERE stm.Id = @Id AND stm.IsDeleted = 0;";

            return await _dbConnection.QueryFirstOrDefaultAsync<StoTypeMasterDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<StoTypeMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 20
                    Id,
                    StoTypeCode,
                    StoTypeName
                FROM Sales.StoTypeMaster
                WHERE IsDeleted = 0 AND IsActive = 1
                  AND (StoTypeCode LIKE @Term OR StoTypeName LIKE @Term)
                ORDER BY StoTypeName ASC;";

            var result = await _dbConnection.QueryAsync<StoTypeMasterLookupDto>(sql, new { Term = $"%{term}%" });
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string stoTypeCode, int? id = null)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.StoTypeMaster
                WHERE StoTypeCode = @StoTypeCode AND IsDeleted = 0
                  AND (@Id IS NULL OR Id != @Id);";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { StoTypeCode = stoTypeCode, Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.StoTypeMaster
                WHERE Id = @Id AND IsDeleted = 0;";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> MovementTypeExistsAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.MovementTypeConfig
                WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0;";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [Sales].[StoHeader] WHERE StoTypeId = @id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { id });
        }

        public async Task<bool> IsStoTypeMasterLinkedAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [Sales].[StoHeader] WHERE StoTypeId = @id AND IsDeleted = 0 AND IsActive = 1
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { id });
        }
    }
}
