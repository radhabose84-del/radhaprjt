#nullable disable
using System.Data;
using InventoryManagement.Application.Common.Interfaces.IUOM;
using InventoryManagement.Application.UOM.Queries.GetUOMs;
using InventoryManagement.Application.UOM.Queries.GetUOMTypeAutoComplete;
using InventoryManagement.Domain.Entities;
using Contracts.Interfaces.Validations.SalesManagement;
using Contracts.Interfaces.Validations.PurchaseManagement;
using Contracts.Interfaces.Validations.MaintenanceManagement;
using Contracts.Interfaces.Validations.WarehouseManagement;
using Contracts.Interfaces.Validations.ProductionManagement;
using Dapper;

namespace InventoryManagement.Infrastructure.Repositories.UOMs
{
    public class UOMQueryRepository : IUOMQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ISalesUomValidation _salesUomValidation;
        private readonly IPurchaseUomValidation _purchaseUomValidation;
        private readonly IMaintenanceUomValidation _maintenanceUomValidation;
        private readonly IWarehouseUomValidation _warehouseUomValidation;
        private readonly IProductionUomValidation _productionUomValidation;

        public UOMQueryRepository(IDbConnection dbConnection,
            ISalesUomValidation salesUomValidation, IPurchaseUomValidation purchaseUomValidation,
            IMaintenanceUomValidation maintenanceUomValidation, IWarehouseUomValidation warehouseUomValidation,
            IProductionUomValidation productionUomValidation)
        {
            _dbConnection = dbConnection;
            _salesUomValidation = salesUomValidation;
            _purchaseUomValidation = purchaseUomValidation;
            _maintenanceUomValidation = maintenanceUomValidation;
            _warehouseUomValidation = warehouseUomValidation;
            _productionUomValidation = productionUomValidation;
        }
        public async Task<(List<UOM>, int)> GetAllUOMAsync(int PageNumber, int PageSize, string SearchTerm)
        {
            var query = $$"""
             DECLARE @TotalCount INT;
             SELECT @TotalCount = COUNT(*) 
               FROM Inventory.UOM 
              WHERE IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (Code LIKE @Search OR UOMName LIKE @Search)")}};

                SELECT 
                Id, 
                Code,
                UOMName,
                SortOrder,
                UOMTypeId,
                IsActive,
                CreatedDate,
                CreatedByName
            FROM Inventory.UOM 
            WHERE 
            IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (Code LIKE @Search OR UOMName LIKE @Search )")}}
                ORDER BY Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;


            var parameters = new
            {
                Search = $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize
            };

            var uom = await _dbConnection.QueryMultipleAsync(query, parameters);
            var uomlist = (await uom.ReadAsync<UOM>()).ToList();
            int totalCount = (await uom.ReadFirstAsync<int>());
            return (uomlist, totalCount);
        }

        public async Task<UOM> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM Inventory.UOM WHERE Id = @Id AND IsDeleted = 0";
            return await _dbConnection.QueryFirstOrDefaultAsync<UOM>(query, new { id });
        }

        public async Task<UOM> GetByUOMNameAsync(string name, int? id = null)
        {
            var query = """
                 SELECT * FROM Inventory.UOM
                 WHERE UOMName = @UOMName AND IsDeleted = 0
                 """;

            var parameters = new DynamicParameters(new { UOMName = name });

            if (id is not null)
            {
                query += " AND Id != @Id";
                parameters.Add("Id", id);
            }

            return await _dbConnection.QueryFirstOrDefaultAsync<UOM>(query, parameters);
        }
        public async Task<List<UOM>> GetUOM(string searchPattern = null, string uomTypeCode = null)
        {
            const string query = @"
                SELECT um.Id, um.UOMName
                FROM Inventory.UOM um
                INNER JOIN Inventory.MiscMaster mm ON um.UOMTypeId = mm.Id
                WHERE um.IsDeleted = 0
                  AND mm.IsDeleted = 0
                  AND um.UOMName LIKE @SearchPattern
                  AND (@UOMTypeCode IS NULL OR mm.Code = @UOMTypeCode)";

            var uoms = await _dbConnection.QueryAsync<UOM>(query, new
            {
                SearchPattern = $"%{searchPattern}%",
                UOMTypeCode = string.IsNullOrWhiteSpace(uomTypeCode) ? null : uomTypeCode
            });
            return uoms.ToList();
        }

        public async Task<List<UOMTypeAutoCompleteDto>> GetUOMType(string searchPattern)
        {
            const string query = @"
                SELECT DISTINCT mm.Id, CAST(mm.Code AS NVARCHAR(255)) AS UomType
                FROM Inventory.UOM um
                INNER JOIN Inventory.MiscMaster mm ON um.UOMTypeId = mm.MiscTypeId
                WHERE um.IsDeleted = 0 AND mm.Code LIKE @SearchPattern";

            var uoms = await _dbConnection.QueryAsync<UOMTypeAutoCompleteDto>(query, new { SearchPattern = $"%{searchPattern}%" });
            return uoms.ToList();
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            var query = "SELECT COUNT(1) FROM Inventory.UOM WHERE Id = @Id AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
            return count == 0;
        }

        public async Task<bool> SoftDeleteValidation(int id)
        {
            const string query = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM [Inventory].[UOMConversion] WHERE FromUOMId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Inventory].[UOMConversion] WHERE ToUOMId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemMaster] WHERE StockUomId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemUOM] WHERE ConversionUOMId = @id)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemPurchase] WHERE PurchaseUomId = @id)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemSale] WHERE UomId = @id)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemInventory] WHERE WeightUomId = @id)
                THEN 1 ELSE 0 END;";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<int>(query, new { id });
            if (result == 1) return true;

            // Cross-module checks
            if (await _salesUomValidation.HasLinkedUomAsync(id)) return true;
            if (await _purchaseUomValidation.HasLinkedUomAsync(id)) return true;
            if (await _maintenanceUomValidation.HasLinkedUomAsync(id)) return true;
            if (await _warehouseUomValidation.HasLinkedUomAsync(id)) return true;
            if (await _productionUomValidation.HasLinkedUomAsync(id)) return true;

            return false;
        }

        public async Task<bool> IsUOMLinkedAsync(int id)
        {
            const string query = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM [Inventory].[UOMConversion] WHERE FromUOMId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Inventory].[UOMConversion] WHERE ToUOMId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemMaster] WHERE StockUomId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemUOM] WHERE ConversionUOMId = @id)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemPurchase] WHERE PurchaseUomId = @id)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemSale] WHERE UomId = @id)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemInventory] WHERE WeightUomId = @id)
                THEN 1 ELSE 0 END;";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<int>(query, new { id });
            if (result == 1) return true;

            // Cross-module checks
            if (await _salesUomValidation.HasActiveUomAsync(id)) return true;
            if (await _purchaseUomValidation.HasActiveUomAsync(id)) return true;
            if (await _maintenanceUomValidation.HasActiveUomAsync(id)) return true;
            if (await _warehouseUomValidation.HasActiveUomAsync(id)) return true;
            if (await _productionUomValidation.HasActiveUomAsync(id)) return true;

            return false;
        }

        public async Task<List<UOMDto>> GetUOMAsync()
            {
                const string query = @"
                    SELECT 
                        Id, 
                        Code, 
                        UOMName, 
                        UOMTypeId, 
                        IsActive 
                    FROM [Inventory].[UOM]
                    WHERE IsDeleted = 0";

                var result = await _dbConnection.QueryAsync<UOMDto>(query);
                return result.ToList();
            }



    }
}