using Dapper;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationMaster;
using InventoryManagement.Application.ItemSpecificationMaster.Dto;
using System.Data;

namespace InventoryManagement.Infrastructure.Repositories.ItemSpecificationMaster
{
    public class ItemSpecificationMasterQueryRepository : IItemSpecificationMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public ItemSpecificationMasterQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<ItemSpecificationMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var offset = (pageNumber - 1) * pageSize;

            var whereClause = "WHERE IsDeleted = 0";
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                whereClause += " AND (SpecificationCode LIKE @SearchTerm OR SpecificationName LIKE @SearchTerm)";
            }

            var sql = $@"
                SELECT * FROM Inventory.ItemSpecificationMaster
                {whereClause}
                ORDER BY [Order] ASC, Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(*) FROM Inventory.ItemSpecificationMaster {whereClause};
            ";

            using var multi = await _dbConnection.QueryMultipleAsync(sql, new
            {
                SearchTerm = $"%{searchTerm}%",
                Offset = offset,
                PageSize = pageSize
            });

            var data = (await multi.ReadAsync<ItemSpecificationMasterDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            return (data, totalCount);
        }

        public async Task<ItemSpecificationMasterDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT * FROM Inventory.ItemSpecificationMaster
                WHERE Id = @Id AND IsDeleted = 0
            ";

            return await _dbConnection.QueryFirstOrDefaultAsync<ItemSpecificationMasterDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<ItemSpecificationMasterLookupDto>> AutocompleteAsync(string term, CancellationToken cancellationToken)
        {
            var sql = @"
                SELECT Id, SpecificationCode, SpecificationName, [Order]
                FROM Inventory.ItemSpecificationMaster
                WHERE IsActive = 1 AND IsDeleted = 0
            ";

            if (!string.IsNullOrWhiteSpace(term))
            {
                sql += " AND (SpecificationCode LIKE @Term OR SpecificationName LIKE @Term)";
            }

            sql += " ORDER BY [Order] ASC, SpecificationName ASC";

            var result = await _dbConnection.QueryAsync<ItemSpecificationMasterLookupDto>(sql, new { Term = $"%{term}%" });
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string specificationCode, int? id = null)
        {
            var sql = "SELECT COUNT(1) FROM Inventory.ItemSpecificationMaster WHERE SpecificationCode = @SpecificationCode AND IsDeleted = 0";
            if (id.HasValue)
            {
                sql += " AND Id != @Id";
            }

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { SpecificationCode = specificationCode, Id = id });
            return count > 0;
        }

        public async Task<bool> NameAlreadyExistsAsync(string specificationName, int? id = null)
        {
            var sql = "SELECT COUNT(1) FROM Inventory.ItemSpecificationMaster WHERE SpecificationName = @SpecificationName AND IsDeleted = 0";
            if (id.HasValue)
            {
                sql += " AND Id != @Id";
            }

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { SpecificationName = specificationName, Id = id });
            return count > 0;
        }

        public async Task<bool> OrderAlreadyExistsAsync(int order, int? id = null)
        {
            var sql = "SELECT COUNT(1) FROM Inventory.ItemSpecificationMaster WHERE [Order] = @Order AND IsDeleted = 0";
            if (id.HasValue)
            {
                sql += " AND Id != @Id";
            }

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Order = order, Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Inventory.ItemSpecificationMaster WHERE Id = @Id AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            // Blocks delete if the master is referenced by any ItemVariantAttribute (junction table)
            // OR by any active ItemSpecificationValue (child records).
            const string sql = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM Inventory.ItemVariantAttribute
                            WHERE SpecificationMasterId = @Id)
                    OR
                    EXISTS (SELECT 1 FROM Inventory.ItemSpecificationValue
                            WHERE SpecificationMasterId = @Id AND IsDeleted = 0)
                THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> IsItemSpecificationMasterLinkedAsync(int id)
        {
            // Blocks inactivate if the master is referenced by any ItemVariantAttribute (junction table).
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Inventory.ItemVariantAttribute
                    WHERE SpecificationMasterId = @Id
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }
    }
}
