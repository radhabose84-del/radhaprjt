using System.Data;
using Dapper;
using InventoryManagement.Application.Common.Interfaces.IProcurementType;
using InventoryManagement.Application.ProcurementType.Dto;

namespace InventoryManagement.Infrastructure.Repositories.ProcurementType
{
    public class ProcurementTypeQueryRepository : IProcurementTypeQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public ProcurementTypeQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<ProcurementTypeDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var offset = (pageNumber - 1) * pageSize;

            var sql = @"
                SELECT
                    Id, ProcurementCode, ProcurementName,
                    IsActive, IsDeleted,
                    CreatedBy, CreatedDate, CreatedByName, CreatedIP,
                    ModifiedBy, ModifiedDate, ModifiedByName, ModifiedIP
                FROM Inventory.ProcurementType
                WHERE IsDeleted = 0";

            var countSql = @"
                SELECT COUNT(*)
                FROM Inventory.ProcurementType
                WHERE IsDeleted = 0";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchFilter = " AND (ProcurementCode LIKE @SearchTerm OR ProcurementName LIKE @SearchTerm)";
                sql += searchFilter;
                countSql += searchFilter;
            }

            sql += " ORDER BY Id DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var parameters = new
            {
                Offset = offset,
                PageSize = pageSize,
                SearchTerm = $"%{searchTerm}%"
            };

            var data = (await _dbConnection.QueryAsync<ProcurementTypeDto>(sql, parameters)).ToList();
            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, parameters);

            return (data, totalCount);
        }

        public async Task<ProcurementTypeDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    Id, ProcurementCode, ProcurementName,
                    IsActive, IsDeleted,
                    CreatedBy, CreatedDate, CreatedByName, CreatedIP,
                    ModifiedBy, ModifiedDate, ModifiedByName, ModifiedIP
                FROM Inventory.ProcurementType
                WHERE Id = @Id AND IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<ProcurementTypeDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<ProcurementTypeLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, ProcurementCode, ProcurementName
                FROM Inventory.ProcurementType
                WHERE IsActive = 1 AND IsDeleted = 0
                    AND (ProcurementCode LIKE @Term OR ProcurementName LIKE @Term)
                ORDER BY ProcurementName ASC";

            var result = await _dbConnection.QueryAsync<ProcurementTypeLookupDto>(sql, new { Term = $"%{term}%" });
            return result.ToList();
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Inventory.ProcurementType WHERE Id = @Id AND IsDeleted = 0
                ) THEN 0 ELSE 1 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<string> GetNextProcurementCodeAsync(string prefix)
        {
            const string sql = @"
                SELECT TOP 1 ProcurementCode
                FROM Inventory.ProcurementType
                WHERE ProcurementCode LIKE @Prefix + '%'
                ORDER BY ProcurementCode DESC";

            var lastCode = await _dbConnection.QueryFirstOrDefaultAsync<string>(sql, new { Prefix = prefix });

            if (string.IsNullOrEmpty(lastCode))
                return prefix + "0001";

            var numericPart = lastCode.Substring(prefix.Length);
            if (int.TryParse(numericPart, out var lastNumber))
                return prefix + (lastNumber + 1).ToString("D4");

            return prefix + "0001";
        }
    }
}
