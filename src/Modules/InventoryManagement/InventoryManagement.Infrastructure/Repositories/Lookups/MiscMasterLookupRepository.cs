using System.Data;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using Dapper;
using InventoryManagement.Domain.Common;

namespace InventoryManagement.Infrastructure.Repositories.Lookups
{
    internal class MiscMasterLookupRepository : IMiscMasterLookup
    {
        private readonly IDbConnection _dbConnection;

        public MiscMasterLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<MiscMasterLookupDto>> GetMiscMasterByIdAsync(string miscType)
        {
            const string sql = @"
                SELECT M.Id, M.Code, M.Description, M.MiscTypeId
                FROM Inventory.MiscMaster AS M
                INNER JOIN Inventory.MiscTypeMaster AS MT ON MT.Id = M.MiscTypeId
                WHERE M.IsDeleted = 0
                  AND MT.IsDeleted = 0
                  AND M.IsActive = 1
                  AND MT.MiscTypeCode = @MiscType
                ORDER BY M.SortOrder, M.Id;";

            var result = await _dbConnection.QueryAsync<MiscMasterLookupDto>(sql, new { MiscType = miscType });
            return result.ToList();
        }

        public async Task<(int? WarehouseTypeId, int? StorageTypeId, int? AreaTypeId, int? OperationTypeId, int? FloorTypeId, int? AisleTypeId, int? RackLevelTypeId)> GetMiscTypeIdsAsync()
        {
            const string sql = @"
                SELECT
                    MAX(CASE WHEN MiscTypeCode = @WarehouseType THEN Id END) AS WarehouseTypeId,
                    MAX(CASE WHEN MiscTypeCode = @StorageType THEN Id END) AS StorageTypeId,
                    MAX(CASE WHEN MiscTypeCode = @AreaType THEN Id END) AS AreaTypeId,
                    MAX(CASE WHEN MiscTypeCode = @OperationType THEN Id END) AS OperationTypeId,
                    MAX(CASE WHEN MiscTypeCode = @FloorType THEN Id END) AS FloorTypeId,
                    MAX(CASE WHEN MiscTypeCode = @AisleType THEN Id END) AS AisleTypeId,
                    MAX(CASE WHEN MiscTypeCode = @RackLevelType THEN Id END) AS RackLevelTypeId
                FROM Inventory.MiscTypeMaster
                WHERE IsDeleted = 0;";

            var row = await _dbConnection.QueryFirstOrDefaultAsync<MiscTypeIdsRow>(sql, new
            {
                WarehouseType = MiscEnumEntity.WarehouseType,
                StorageType = MiscEnumEntity.StorageType,
                AreaType = MiscEnumEntity.AreaType,
                OperationType = MiscEnumEntity.OperationType,
                FloorType = MiscEnumEntity.Floor,
                AisleType = MiscEnumEntity.WarehouseAisle,
                RackLevelType = MiscEnumEntity.WarehouseRackLevel
            });

            if (row is null)
            {
                return (null, null, null, null, null, null, null);
            }

            return (
                row.WarehouseTypeId,
                row.StorageTypeId,
                row.AreaTypeId,
                row.OperationTypeId,
                row.FloorTypeId,
                row.AisleTypeId,
                row.RackLevelTypeId);
        }

        private sealed class MiscTypeIdsRow
        {
            public int? WarehouseTypeId { get; init; }
            public int? StorageTypeId { get; init; }
            public int? AreaTypeId { get; init; }
            public int? OperationTypeId { get; init; }
            public int? FloorTypeId { get; init; }
            public int? AisleTypeId { get; init; }
            public int? RackLevelTypeId { get; init; }
        }
    }
}
