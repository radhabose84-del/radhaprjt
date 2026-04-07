#nullable disable
using System.Data;
using InventoryManagement.Application.Common.Interfaces.IHSNMaster;
using InventoryManagement.Application.HSNMaster.Queries.GetAllHSNMaster;
using InventoryManagement.Application.HSNMaster.Queries.GetHSNMasterAutoComplete;
using Contracts.Interfaces.Validations.SalesManagement;
using Contracts.Interfaces.Validations.PurchaseManagement;
using Dapper;

namespace InventoryManagement.Infrastructure.Repositories.HSNMaster
{
    public class HSNMasterQueryRepository : IHSNMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ISalesHsnValidation _salesHsnValidation;
        private readonly IPurchaseHsnValidation _purchaseHsnValidation;

        public HSNMasterQueryRepository(IDbConnection dbConnection,
            ISalesHsnValidation salesHsnValidation, IPurchaseHsnValidation purchaseHsnValidation)
        {
            _dbConnection = dbConnection;
            _salesHsnValidation = salesHsnValidation;
            _purchaseHsnValidation = purchaseHsnValidation;
        }
        public async Task<(List<HSNMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string searchTerm)
        {
            var query = $$"""
                DECLARE @TotalCount INT;

                SELECT @TotalCount = COUNT(*)
                FROM Inventory.HSNMaster h
                LEFT JOIN Inventory.MiscMaster m ON h.GSTCategoryId = m.Id
                INNER JOIN Inventory.MiscMaster ht ON h.TypeId = ht.Id
                WHERE h.IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (h.HSNCode LIKE @Search OR h.Description LIKE @Search OR m.Description LIKE @Search)")}};
                
                SELECT 
                    h.Id, h.TypeId,ht.Code AS Type, h.HSNCode, h.Description, h.GSTCategoryId,
                    m.Description AS GstCategoryName,
                    h.GstPercentage AS GSTPercentage,
                    h.CgstPercentage AS CGSTPercentage,
                    h.SgstPercentage AS SGSTPercentage,
                    h.IgstPercentage AS IGSTPercentage,
                    h.ValidFrom,
                    h.IsActive,
                    h.IsDeleted,
                    h.CreatedBy,
                    h.CreatedDate,
                    h.CreatedByName,
                    h.CreatedIP,
                    h.ModifiedBy,
                    h.ModifiedDate,
                    h.ModifiedByName,
                    h.ModifiedIP
                FROM Inventory.HSNMaster h
                LEFT JOIN Inventory.MiscMaster m ON h.GSTCategoryId = m.Id
                INNER JOIN Inventory.MiscMaster ht ON h.TypeId = ht.Id
                WHERE h.IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (h.HSNCode LIKE @Search OR h.Description LIKE @Search OR m.Description LIKE @Search)")}}
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var hsnList = (await result.ReadAsync<HSNMasterDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            return (hsnList, totalCount);
        }

        public async Task<HSNMasterDto> GetByIdAsync(int id)
        {
            var query = @"
                SELECT h.Id, h.TypeId ,m2.Code AS Type, h.HSNCode, h.Description, h.GSTCategoryId,
                    m1.Description AS GstCategoryName, m1.Description AS TypeName,
                    h.GSTPercentage, h.CGSTPercentage, h.SGSTPercentage, h.IGSTPercentage,
                    h.ValidFrom, h.IsActive, h.IsDeleted, h.CreatedBy, h.CreatedDate,
                    h.CreatedByName, h.CreatedIP, h.ModifiedBy, h.ModifiedDate,
                    h.ModifiedByName, h.ModifiedIP
                FROM Inventory.HSNMaster h
                LEFT JOIN Inventory.MiscMaster m1 ON h.GSTCategoryId = m1.Id
                LEFT JOIN Inventory.MiscMaster m2 ON h.TypeId = m2.Id
                WHERE h.Id = @Id AND h.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<HSNMasterDto>(query, new { Id = id });
        }

        // public async Task<List<GetHSNMasterAutoCompleteDto>> GetHSNMasterAutoCompleteAsync(string searchPattern)
        // {
        //     const string query = @"
        //         SELECT TOP 15 
        //             Id, 
        //             HSNCode, 
        //             Description AS HSNDescription
        //         FROM Inventory.HSNMaster
        //         WHERE IsDeleted = 0 
        //         AND (HSNCode LIKE @SearchPattern OR Description LIKE @SearchPattern)
        //         ORDER BY HSNCode";

        //     var parameters = new
        //     {
        //         SearchPattern = $"%{searchPattern}%"
        //     };

        //     var result = await _dbConnection.QueryAsync<GetHSNMasterAutoCompleteDto>(query, parameters);
        //     return result.ToList();
        // }

        public async Task<List<GetHSNMasterAutoCompleteDto>> GetHSNMasterAutoCompleteAsync(
            string search = null,                 // null/empty => no text filter
            string typeCode = null         // "HSN", "SAC", or null/empty => both
        )
        {
            const string sql = @"
        SELECT 
            h.Id,
            h.HSNCode            AS HSNCode,
            h.[Description]      AS HSNDescription,
            m.[Code]             AS TypeCode,     -- 'HSN' or 'SAC'
            m.Id                 AS TypeId
        FROM Inventory.HSNMaster h
        JOIN Inventory.MiscMaster m
        ON m.Id = h.TypeId
        JOIN Inventory.MiscTypeMaster mt
        ON mt.Id = m.MiscTypeId
        AND mt.MiscTypeCode = 'HSNType'
        WHERE
            h.IsDeleted = 0
            AND (@FilterCode IS NULL OR @FilterCode = '' OR m.[Code] = @FilterCode)
            AND (
                @SearchLike IS NULL
                OR h.HSNCode      LIKE @SearchLike
                OR h.[Description] LIKE @SearchLike
            )
        ORDER BY h.HSNCode;";

            var searchLike = string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%";
            var filterCode = string.IsNullOrWhiteSpace(typeCode) ? null : typeCode.Trim().ToUpperInvariant();

            var result = await _dbConnection.QueryAsync<GetHSNMasterAutoCompleteDto>(
                sql,
                new { SearchLike = searchLike, FilterCode = filterCode }
            );

            return result.ToList();
        }

       public async Task<bool> AlreadyExistsAsync(string hsnCode, int? id = null)
        {
            const string baseSql = @"
                SELECT COUNT(1) 
                FROM Inventory.HSNMaster
                WHERE HSNCode = @HSNCode 
                AND IsDeleted = 0";

            var sql = baseSql;

            if (id.HasValue && id.Value > 0)
            {
                sql += " AND Id != @Id";
            }

            var parameters = new
            {
                HSNCode = hsnCode.Trim(),  // ✅ Prevent whitespace causing duplicates
                Id = id
            };

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, parameters);
            return count > 0;
        }
        public async Task<bool> NotFoundAsync(int id)
        {
            const string query = @"SELECT COUNT(1) 
                                    FROM Inventory.HSNMaster 
                                    WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });

            return count == 0;
        }        
            
         public async Task<bool> SoftDeleteValidation(int id)
        {
            const string query = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [Inventory].[ItemMaster]
                    WHERE HSNId = @id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END;";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<int>(query, new { id });
            if (result == 1) return true;

            // Cross-module checks
            if (await _salesHsnValidation.HasLinkedHsnAsync(id)) return true;
            if (await _purchaseHsnValidation.HasLinkedHsnAsync(id)) return true;

            return false;
        }

        public async Task<bool> IsHSNMasterLinkedAsync(int id)
        {
            const string query = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [Inventory].[ItemMaster]
                    WHERE HSNId = @id AND IsDeleted = 0 AND IsActive = 1
                ) THEN 1 ELSE 0 END;";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<int>(query, new { id });
            if (result == 1) return true;

            // Cross-module checks
            if (await _salesHsnValidation.HasActiveHsnAsync(id)) return true;
            if (await _purchaseHsnValidation.HasActiveHsnAsync(id)) return true;

            return false;
        }

        public async Task<bool> FKColumnValidation(int hsnMasterId)
        {
            // Find all referencing tables and columns dynamically
            var fkQuery = @"
                SELECT 
                    OBJECT_NAME(fk.parent_object_id) AS TableName,
                    c1.name AS ColumnName
                FROM sys.foreign_keys fk
                INNER JOIN sys.foreign_key_columns fkc 
                    ON fk.object_id = fkc.constraint_object_id
                INNER JOIN sys.columns c1 
                    ON fkc.parent_object_id = c1.object_id 
                    AND fkc.parent_column_id = c1.column_id
                WHERE OBJECT_NAME(fk.referenced_object_id) = 'HSNMaster';";

            var fkList = await _dbConnection.QueryAsync<(string TableName, string ColumnName)>(fkQuery);

            foreach (var fk in fkList)
            {
                var sql = $@"
                    SELECT COUNT(1)
                    FROM Inventory.{fk.TableName}
                    WHERE {fk.ColumnName} = @Id
                    AND IsDeleted = 0";

                var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = hsnMasterId });

                if (count > 0)
                {
                    return true; 
                }
            }
            return false; 
        }



    }
}