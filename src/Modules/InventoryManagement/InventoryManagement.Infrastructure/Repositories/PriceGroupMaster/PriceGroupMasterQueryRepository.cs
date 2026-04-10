using System.Data;
using Contracts.Dtos.Lookups.Inventory;
using Dapper;
using InventoryManagement.Application.Common.Interfaces.IPriceGroupMaster;
using InventoryManagement.Application.PriceGroupMaster.Dto;

namespace InventoryManagement.Infrastructure.Repositories.PriceGroupMaster
{
    public class PriceGroupMasterQueryRepository : IPriceGroupMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public PriceGroupMasterQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<PriceGroupMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var sql = $$"""
                DECLARE @TotalCount INT;

                SELECT @TotalCount = COUNT(*)
                FROM Inventory.PriceGroupMaster
                WHERE IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (PriceGroupCode LIKE @Search OR PriceGroupName LIKE @Search OR Description LIKE @Search)")}};

                SELECT
                    Id,
                    PriceGroupCode,
                    PriceGroupName,
                    Description,
                    EffectiveFrom,
                    EffectiveTo,
                    IsActive,
                    IsDeleted,
                    CreatedBy,
                    CreatedDate,
                    CreatedByName,
                    ModifiedBy,
                    ModifiedDate,
                    ModifiedByName
                FROM Inventory.PriceGroupMaster
                WHERE IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (PriceGroupCode LIKE @Search OR PriceGroupName LIKE @Search OR Description LIKE @Search)")}}
                ORDER BY Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var result = await _dbConnection.QueryMultipleAsync(sql, parameters);
            var list = (await result.ReadAsync<PriceGroupMasterDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            return (list, totalCount);
        }

        public async Task<PriceGroupMasterDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    Id,
                    PriceGroupCode,
                    PriceGroupName,
                    Description,
                    EffectiveFrom,
                    EffectiveTo,
                    IsActive,
                    IsDeleted,
                    CreatedBy,
                    CreatedDate,
                    CreatedByName,
                    ModifiedBy,
                    ModifiedDate,
                    ModifiedByName
                FROM Inventory.PriceGroupMaster
                WHERE Id = @Id AND IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<PriceGroupMasterDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<PriceGroupMasterLookupDto>> AutocompleteAsync(string term, CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT TOP 20
                    Id,
                    PriceGroupCode,
                    PriceGroupName
                FROM Inventory.PriceGroupMaster
                WHERE IsActive = 1
                  AND IsDeleted = 0
                  AND (
                       @Search IS NULL
                    OR PriceGroupCode LIKE @Search
                    OR PriceGroupName LIKE @Search
                  )
                ORDER BY PriceGroupCode";

            var search = string.IsNullOrWhiteSpace(term) ? null : $"%{term.Trim()}%";

            var result = await _dbConnection.QueryAsync<PriceGroupMasterLookupDto>(
                new CommandDefinition(sql, new { Search = search }, cancellationToken: cancellationToken));

            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string priceGroupCode, int? id = null)
        {
            // ⭐ Soft-delete safe: only checks live (IsDeleted = 0) rows.
            // This pairs with the filtered unique index on PriceGroupCode so a user
            // CAN re-create a code whose only previous occupant was soft-deleted.
            const string baseSql = @"
                SELECT COUNT(1)
                FROM Inventory.PriceGroupMaster
                WHERE PriceGroupCode = @PriceGroupCode
                  AND IsDeleted = 0";

            var sql = baseSql;
            if (id.HasValue && id.Value > 0)
            {
                sql += " AND Id != @Id";
            }

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new
            {
                PriceGroupCode = priceGroupCode.Trim(),
                Id = id
            });

            return count > 0;
        }

        public async Task<bool> NameAlreadyExistsAsync(string priceGroupName, int? id = null)
        {
            // ⭐ Soft-delete safe — same reasoning as AlreadyExistsAsync above.
            const string baseSql = @"
                SELECT COUNT(1)
                FROM Inventory.PriceGroupMaster
                WHERE PriceGroupName = @PriceGroupName
                  AND IsDeleted = 0";

            var sql = baseSql;
            if (id.HasValue && id.Value > 0)
            {
                sql += " AND Id != @Id";
            }

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new
            {
                PriceGroupName = priceGroupName.Trim(),
                Id = id
            });

            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Inventory.PriceGroupMaster
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }
    }
}
