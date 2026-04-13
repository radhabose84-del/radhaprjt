using Dapper;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Dto;
using System.Data;

namespace InventoryManagement.Infrastructure.Repositories.ItemSpecificationValue
{
    public class ItemSpecificationValueQueryRepository : IItemSpecificationValueQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public ItemSpecificationValueQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<ItemSpecificationValueDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var offset = (pageNumber - 1) * pageSize;

            var whereClause = "WHERE v.IsDeleted = 0";
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                whereClause += " AND (v.SpecificationValue LIKE @SearchTerm OR m.SpecificationName LIKE @SearchTerm)";
            }

            var sql = $@"
                SELECT
                    v.Id,
                    v.SpecificationMasterId,
                    m.SpecificationName AS SpecificationMasterName,
                    v.SpecificationValue,
                    v.IsActive,
                    v.IsDeleted,
                    v.CreatedBy,
                    v.CreatedDate,
                    v.CreatedByName,
                    v.CreatedIP,
                    v.ModifiedBy,
                    v.ModifiedDate,
                    v.ModifiedByName,
                    v.ModifiedIP
                FROM Inventory.ItemSpecificationValue v
                LEFT JOIN Inventory.ItemSpecificationMaster m
                    ON v.SpecificationMasterId = m.Id AND m.IsDeleted = 0
                {whereClause}
                ORDER BY v.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(*)
                FROM Inventory.ItemSpecificationValue v
                LEFT JOIN Inventory.ItemSpecificationMaster m
                    ON v.SpecificationMasterId = m.Id AND m.IsDeleted = 0
                {whereClause};
            ";

            using var multi = await _dbConnection.QueryMultipleAsync(sql, new
            {
                SearchTerm = $"%{searchTerm}%",
                Offset = offset,
                PageSize = pageSize
            });

            var data = (await multi.ReadAsync<ItemSpecificationValueDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            return (data, totalCount);
        }

        public async Task<ItemSpecificationValueDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    v.Id,
                    v.SpecificationMasterId,
                    m.SpecificationName AS SpecificationMasterName,
                    v.SpecificationValue,
                    v.IsActive,
                    v.IsDeleted,
                    v.CreatedBy,
                    v.CreatedDate,
                    v.CreatedByName,
                    v.CreatedIP,
                    v.ModifiedBy,
                    v.ModifiedDate,
                    v.ModifiedByName,
                    v.ModifiedIP
                FROM Inventory.ItemSpecificationValue v
                LEFT JOIN Inventory.ItemSpecificationMaster m
                    ON v.SpecificationMasterId = m.Id AND m.IsDeleted = 0
                WHERE v.Id = @Id AND v.IsDeleted = 0
            ";

            return await _dbConnection.QueryFirstOrDefaultAsync<ItemSpecificationValueDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<ItemSpecificationValueDto>> GetBySpecificationMasterIdAsync(int specificationMasterId, CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT
                    v.Id,
                    v.SpecificationMasterId,
                    m.SpecificationName AS SpecificationMasterName,
                    v.SpecificationValue,
                    v.IsActive,
                    v.IsDeleted,
                    v.CreatedBy,
                    v.CreatedDate,
                    v.CreatedByName,
                    v.CreatedIP,
                    v.ModifiedBy,
                    v.ModifiedDate,
                    v.ModifiedByName,
                    v.ModifiedIP
                FROM Inventory.ItemSpecificationValue v
                LEFT JOIN Inventory.ItemSpecificationMaster m
                    ON v.SpecificationMasterId = m.Id AND m.IsDeleted = 0
                WHERE v.SpecificationMasterId = @SpecificationMasterId
                  AND v.IsActive = 1
                  AND v.IsDeleted = 0
                ORDER BY v.SpecificationValue ASC;
            ";

            var result = await _dbConnection.QueryAsync<ItemSpecificationValueDto>(sql, new { SpecificationMasterId = specificationMasterId });
            return result.ToList();
        }

        public async Task<IReadOnlyList<ItemSpecificationValueLookupDto>> AutocompleteAsync(string term, CancellationToken cancellationToken)
        {
            var sql = @"
                SELECT Id, SpecificationMasterId, SpecificationValue
                FROM Inventory.ItemSpecificationValue
                WHERE IsActive = 1 AND IsDeleted = 0
            ";

            if (!string.IsNullOrWhiteSpace(term))
            {
                sql += " AND SpecificationValue LIKE @Term";
            }

            sql += " ORDER BY SpecificationValue ASC";

            var result = await _dbConnection.QueryAsync<ItemSpecificationValueLookupDto>(sql, new { Term = $"%{term}%" });
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(int specificationMasterId, string specificationValue, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1) FROM Inventory.ItemSpecificationValue
                WHERE SpecificationMasterId = @SpecificationMasterId
                  AND SpecificationValue = @SpecificationValue
                  AND IsDeleted = 0";
            if (id.HasValue)
            {
                sql += " AND Id != @Id";
            }

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new
            {
                SpecificationMasterId = specificationMasterId,
                SpecificationValue = specificationValue,
                Id = id
            });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Inventory.ItemSpecificationValue WHERE Id = @Id AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> SpecificationMasterExistsAsync(int specificationMasterId)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Inventory.ItemSpecificationMaster
                WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = specificationMasterId });
            return count > 0;
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            // Blocks delete if this value is referenced by any ItemVariantValue (junction)
            // OR by any active ItemItemSpecification (child mapping).
            const string sql = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM Inventory.ItemVariantValue
                            WHERE SpecificationValueId = @Id)
                    OR
                    EXISTS (SELECT 1 FROM Inventory.ItemItemSpecification
                            WHERE SpecificationValueId = @Id AND IsDeleted = 0)
                THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> IsItemSpecificationValueLinkedAsync(int id)
        {
            // Blocks inactivate if this value is referenced by any ItemVariantValue (junction)
            // OR by any active ItemItemSpecification (child mapping).
            const string sql = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM Inventory.ItemVariantValue
                            WHERE SpecificationValueId = @Id)
                    OR
                    EXISTS (SELECT 1 FROM Inventory.ItemItemSpecification
                            WHERE SpecificationValueId = @Id AND IsDeleted = 0 AND IsActive = 1)
                THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }
    }
}
