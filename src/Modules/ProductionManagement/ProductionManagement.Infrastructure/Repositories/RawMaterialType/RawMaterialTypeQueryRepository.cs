using System.Data;
using Contracts.Dtos.Lookups.Production;
using Dapper;
using ProductionManagement.Application.Common.Interfaces.IRawMaterialType;
using ProductionManagement.Application.RawMaterialType.Dto;

namespace ProductionManagement.Infrastructure.Repositories.RawMaterialType
{
    public class RawMaterialTypeQueryRepository : IRawMaterialTypeQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public RawMaterialTypeQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<RawMaterialTypeDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var offset = (pageNumber - 1) * pageSize;

            const string sql = @"
                DECLARE @TotalCount INT;

                SELECT @TotalCount = COUNT(*)
                FROM Production.RawMaterialType
                WHERE IsDeleted = 0
                  AND (@SearchTerm IS NULL
                       OR RawMaterialTypeCode LIKE @SearchTerm
                       OR RawMaterialTypeName LIKE @SearchTerm
                       OR Description LIKE @SearchTerm);

                SELECT
                    Id,
                    RawMaterialTypeCode,
                    RawMaterialTypeName,
                    Description,
                    EffectiveFrom,
                    EffectiveTo,
                    IsActive,
                    IsDeleted,
                    CreatedBy,
                    CreatedDate,
                    CreatedByName,
                    CreatedIP,
                    ModifiedBy,
                    ModifiedDate,
                    ModifiedByName,
                    ModifiedIP
                FROM Production.RawMaterialType
                WHERE IsDeleted = 0
                  AND (@SearchTerm IS NULL
                       OR RawMaterialTypeCode LIKE @SearchTerm
                       OR RawMaterialTypeName LIKE @SearchTerm
                       OR Description LIKE @SearchTerm)
                ORDER BY Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new
            {
                SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? (object?)null : $"%{searchTerm}%",
                Offset = offset,
                PageSize = pageSize
            };

            using var multi = await _dbConnection.QueryMultipleAsync(sql, parameters);
            var list = (await multi.ReadAsync<RawMaterialTypeDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            return (list, totalCount);
        }

        public async Task<RawMaterialTypeDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    Id,
                    RawMaterialTypeCode,
                    RawMaterialTypeName,
                    Description,
                    EffectiveFrom,
                    EffectiveTo,
                    IsActive,
                    IsDeleted,
                    CreatedBy,
                    CreatedDate,
                    CreatedByName,
                    CreatedIP,
                    ModifiedBy,
                    ModifiedDate,
                    ModifiedByName,
                    ModifiedIP
                FROM Production.RawMaterialType
                WHERE Id = @Id AND IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<RawMaterialTypeDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<RawMaterialTypeLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 20
                    Id,
                    RawMaterialTypeCode,
                    RawMaterialTypeName
                FROM Production.RawMaterialType
                WHERE IsActive = 1
                  AND IsDeleted = 0
                  AND (
                       @Search IS NULL
                    OR RawMaterialTypeCode LIKE @Search
                    OR RawMaterialTypeName LIKE @Search
                  )
                ORDER BY RawMaterialTypeName ASC";

            var search = string.IsNullOrWhiteSpace(term) ? null : $"%{term.Trim()}%";

            var result = await _dbConnection.QueryAsync<RawMaterialTypeLookupDto>(
                new CommandDefinition(sql, new { Search = search }, cancellationToken: ct));

            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string rawMaterialTypeCode, int? id = null)
        {
            // ⭐ Soft-delete safe: only checks live (IsDeleted = 0) rows.
            // Pairs with the filtered unique index on RawMaterialTypeCode so a user CAN
            // re-create a code whose only previous occupant was soft-deleted.
            const string baseSql = @"
                SELECT COUNT(1)
                FROM Production.RawMaterialType
                WHERE RawMaterialTypeCode = @RawMaterialTypeCode
                  AND IsDeleted = 0";

            var sql = baseSql;
            if (id.HasValue && id.Value > 0)
            {
                sql += " AND Id != @Id";
            }

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new
            {
                RawMaterialTypeCode = rawMaterialTypeCode.Trim(),
                Id = id
            });

            return count > 0;
        }

        public async Task<bool> NameAlreadyExistsAsync(string rawMaterialTypeName, int? id = null)
        {
            // ⭐ Soft-delete safe — same reasoning as AlreadyExistsAsync above.
            const string baseSql = @"
                SELECT COUNT(1)
                FROM Production.RawMaterialType
                WHERE RawMaterialTypeName = @RawMaterialTypeName
                  AND IsDeleted = 0";

            var sql = baseSql;
            if (id.HasValue && id.Value > 0)
            {
                sql += " AND Id != @Id";
            }

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new
            {
                RawMaterialTypeName = rawMaterialTypeName.Trim(),
                Id = id
            });

            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Production.RawMaterialType
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public Task<bool> IsRawMaterialTypeLinkedAsync(int id)
        {
            // Reserved for Rule #25 — currently always returns false (no dependent entity references
            // RawMaterialTypeId yet). When a future entity adds a FK to RawMaterialType, replace this
            // body with an EXISTS query against the dependent table(s) filtered by IsDeleted = 0
            // (and IsActive = 1 for the inactivate guard).
            _ = id;
            return Task.FromResult(false);
        }
    }
}
