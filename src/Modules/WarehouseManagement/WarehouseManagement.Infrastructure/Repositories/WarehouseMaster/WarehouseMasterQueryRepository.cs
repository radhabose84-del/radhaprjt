#nullable disable
using System.Data;
using Contracts.Interfaces;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.GetAllWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Queries.GetParentWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Queries.GetWareMasterAutoComplete;
using Dapper;

namespace WarehouseManagement.Infrastructure.Repositories.WarehouseMaster
{
    public class WarehouseMasterQueryRepository : IWarehouseMasterQueryRepository
    {


        private readonly IDbConnection _dbConnection;

         private readonly IIPAddressService _ipAddressService;

        public WarehouseMasterQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
        }

        public async Task<(List<WarehouseMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string searchTerm)
        {

             var unitId = _ipAddressService.GetUnitId() ?? 0;

            var sql = @"
                DECLARE @TotalCount INT;
                DECLARE @Search NVARCHAR(200) = @SearchParam;

                -- total count
                SELECT @TotalCount = COUNT(*)
                FROM Warehouse.WarehouseMaster w
                WHERE w.IsDeleted = 0  
                AND (
                    @Search IS NULL OR @Search = '' 
                    OR (w.WarehouseCode LIKE @Search OR w.WarehouseName LIKE @Search OR w.ContactPersonName LIKE @Search)
                );

                -- page ids into a temp table (CTE scope issue avoided)
                IF OBJECT_ID('tempdb..#PagedIds') IS NOT NULL DROP TABLE #PagedIds;
                CREATE TABLE #PagedIds (Id INT PRIMARY KEY);

                INSERT INTO #PagedIds(Id)
                SELECT w.Id
                FROM Warehouse.WarehouseMaster w
                WHERE w.IsDeleted = 0  
                AND (
                    @Search IS NULL OR @Search = '' 
                    OR (w.WarehouseCode LIKE @Search OR w.WarehouseName LIKE @Search OR w.ContactPersonName LIKE @Search)
                )
                ORDER BY w.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                -- page details
                SELECT 
                    w.Id,
                    w.WarehouseCode,
                    w.WarehouseName,
                    w.UnitId,
                    w.ParentWarehouseId,
                    w.IsGroup,
                    w.IsVirtualWarehouse,
                    w.WarehouseTypeId,
                    w.DepartmentId,
                    w.StorageTypeId,
                    w.AreaTypeId,
                    w.OperationTypeId,
                    w.CapacityUOMId,
                    w.AccountId,
                    w.ContactPersonName,
                    w.MobileNumber,
                    w.Email,
                    w.AddressLine1,
                    w.AddressLine2,
                    w.CityId,
                    w.StateId,
                    w.CountryId,
                    w.Pincode,
                    w.IsScrapWarehouse,
                    w.IsTransitWarehouse,
                    w.MaxCapacity,
                    w.IsDefaultStockEntry,
                    w.IsActive,
                    w.IsDeleted,
                    w.CreatedBy,
                    w.CreatedDate,
                    w.CreatedByName,
                    w.CreatedIP,
                    w.ModifiedBy,
                    w.ModifiedDate,
                    w.ModifiedByName,
                    w.ModifiedIP
                FROM Warehouse.WarehouseMaster w
                JOIN #PagedIds p ON p.Id = w.Id
                WHERE w.IsDeleted = 0   AND w.unitId = @UnitId
             
                ORDER BY w.Id DESC;  -- keep same order as paging

                -- allowed item groups for just this page
                SELECT m.WarehouseId, m.ItemGroupId
                FROM Warehouse.WarehouseItemGroupMapping m
                JOIN #PagedIds p ON p.Id = m.WarehouseId
                WHERE m.IsDeleted = 0 ;

                -- total count out
                SELECT @TotalCount AS TotalCount;
            ";
            var parameters = new
            {
                UnitId = unitId, 
                SearchParam = string.IsNullOrWhiteSpace(searchTerm) ? null : $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };
            using var multi = await _dbConnection.QueryMultipleAsync(sql, parameters);
            var warehouses = (await multi.ReadAsync<WarehouseMasterDto>()).ToList();
            var mappings = (await multi.ReadAsync<(int WarehouseId, int ItemGroupId)>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            var mapLookup = mappings
                .GroupBy(x => x.WarehouseId)
                .ToDictionary(g => g.Key, g => g.Select(y => y.ItemGroupId).ToList());

            foreach (var w in warehouses)
                w.AllowedItemGroupIds = mapLookup.TryGetValue(w.Id, out var ids) ? ids : new List<int>();

            return (warehouses, totalCount);
        }
        public async Task<WarehouseMasterDto> GetByIdAsync(int id)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            var sql = @"
               
                SELECT 
                    wm.Id, wm.WarehouseCode, wm.WarehouseName, wm.UnitId, wm.IsVirtualWarehouse,
                    wm.ParentWarehouseId, wm.IsGroup, wm.WarehouseTypeId, wm.DepartmentId,
                    wm.StorageTypeId, wm.AreaTypeId, wm.OperationTypeId, wm.CapacityUOMId, wm.AccountId,
                    wm.ContactPersonName, wm.MobileNumber, wm.Email,
                    wm.AddressLine1, wm.AddressLine2, wm.CityId, wm.StateId,
                    wm.CountryId, wm.Pincode, wm.IsScrapWarehouse, wm.IsTransitWarehouse,
                    wm.MaxCapacity, wm.IsDefaultStockEntry, wm.IsActive, wm.IsDeleted,
                    wm.CreatedBy, wm.CreatedDate, wm.CreatedByName, wm.CreatedIP,
                    wm.ModifiedBy, wm.ModifiedDate, wm.ModifiedByName, wm.ModifiedIP
                FROM Warehouse.WarehouseMaster wm
                WHERE wm.Id = @Id AND wm.IsDeleted = 0  AND wm.unitId = @UnitId; 

               
                SELECT wigm.ItemGroupId
                FROM Warehouse.WarehouseItemGroupMapping wigm
                WHERE wigm.WarehouseId = @Id
                
                AND wigm.IsDeleted = 0 ;
            ";

            using var multi = await _dbConnection.QueryMultipleAsync(sql, new { Id = id , unitId });

            var warehouse = await multi.ReadFirstOrDefaultAsync<WarehouseMasterDto>();
            if (warehouse == null) return null;

            warehouse.AllowedItemGroupIds = (await multi.ReadAsync<int>()).ToList();
            return warehouse;
        }
        public async Task<bool> ExistsByNameAsync(string warehouseName, int? excludeId = null)
        {
             var unitId = _ipAddressService.GetUnitId() ?? 0;          
          
            var sql = @"
                SELECT COUNT(1)
                FROM [Warehouse].[WarehouseMaster] WITH (NOLOCK)
                WHERE IsDeleted = 0  AND unitId = @UnitId             
                AND UPPER(LTRIM(RTRIM(WarehouseName))) = UPPER(LTRIM(RTRIM(@WarehouseName)))";

            var p = new DynamicParameters(new { WarehouseName = warehouseName  , unitId });

            if (excludeId is not null)
            {
                // Skip the current record being updated
                sql += " AND Id != @Id";  // change Id to your PK column if different
                p.Add("Id", excludeId);
            }

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, p );
            return count > 0; // true = duplicate exists
        }
        public async Task<List<GetWarehouseAutoCompleteDto>> GetWarehouseMasterAutoCompletes(string searchPattern)
        {
               var unitId = _ipAddressService.GetUnitId() ?? 0;

            //Old Query Not Added ParentWarehouseId and ParentWarehouseName
            //     SELECT
            //     w.Id,
            //     w.WarehouseCode,
            //     w.WarehouseName
            // FROM   [Warehouse].[WarehouseMaster] w
            // WHERE  w.IsDeleted = 0 AND w.unitId = @UnitId
            // AND (@Term = '' 
            //     OR w.WarehouseCode LIKE '%' + @Term + '%'
            //     OR w.WarehouseName LIKE '%' + @Term + '%')
            // ORDER BY w.WarehouseCode, w.WarehouseName;

             //New Query Added ParentWarehouseId and ParentWarehouseName
            const string sql = @"
           

            SELECT 
                w.Id,
                w.WarehouseCode,
                w.WarehouseName,
                w.ParentWarehouseId,
                pw.WarehouseName AS ParentWarehouseName
            FROM [Warehouse].[WarehouseMaster] w
            LEFT JOIN [Warehouse].[WarehouseMaster] pw 
                ON w.ParentWarehouseId = pw.Id
            WHERE 

                w.IsDeleted = 0  AND w.IsActive = 1
                AND w.IsActive = 1
                AND w.UnitId = @UnitId
                AND (
                    @Term = '' 
                    OR w.WarehouseCode LIKE '%' + @Term + '%'
                    OR w.WarehouseName LIKE '%' + @Term + '%'
                    OR pw.WarehouseName LIKE '%' + @Term + '%'
                )
            ORDER BY 
                w.WarehouseCode, 
                w.WarehouseName;
            
            ";

            var rows = await _dbConnection.QueryAsync<GetWarehouseAutoCompleteDto>(sql, new
            {
                UnitId = unitId,
                Term = (searchPattern ?? string.Empty).Trim(),

            });
            return rows.ToList();
        }
        public async Task<List<GetParentWarehouseDto>> GetParentWarehouseMaster()
        {
               var unitId = _ipAddressService.GetUnitId() ?? 0;
            const string sql = @"
                                            SELECT
                                                w.Id   AS Id,
                                                w.WarehouseCode AS ParentWarehouseCode,
                                                w.WarehouseName   AS ParentWarehouseName
                                            FROM [Warehouse].[WarehouseMaster] AS w
                                            WHERE w.IsDeleted = 0
                                            AND w.IsActive  = 1
                                            AND w.ParentWarehouseId IS NULL   
                                            AND w.unitId = @UnitId
                                            ORDER BY w.WarehouseName, w.WarehouseCode; ";

            var rows = await _dbConnection.QueryAsync<GetParentWarehouseDto>(
                new CommandDefinition(sql, new { UnitId = unitId }, cancellationToken: default));
            return rows.AsList();

        }
        public async Task<List<WarehouseMasterDto>> GetwarehouseAsync()
        {
               var unitId = _ipAddressService.GetUnitId() ?? 0;
            const string sql = @"SELECT Id, WarehouseCode, WarehouseName, UnitId
                                        FROM Warehouse.WarehouseMaster
                                        WHERE IsDeleted = 0 AND unitId = @UnitId";

            var items = (await _dbConnection.QueryAsync<WarehouseMasterDto>(sql , new { UnitId = unitId })).ToList();
            return items;
        }

        public async Task<List<GetWarehouseAutoCompleteDto>> GetByUnitIdAsync(int unitId)
        {
            const string sql = @"
                SELECT
                    w.Id,
                    w.WarehouseCode,
                    w.WarehouseName,
                    w.ParentWarehouseId,
                    pw.WarehouseName AS ParentWarehouseName
                FROM [Warehouse].[WarehouseMaster] w
                LEFT JOIN [Warehouse].[WarehouseMaster] pw
                    ON w.ParentWarehouseId = pw.Id
                WHERE w.IsDeleted = 0
                    AND w.IsActive = 1
                    AND w.UnitId = @UnitId
                ORDER BY w.WarehouseCode, w.WarehouseName;
            ";

            var rows = await _dbConnection.QueryAsync<GetWarehouseAutoCompleteDto>(sql, new { UnitId = unitId });
            return rows.ToList();
        }
    }
}