using System.Data;
using System.Threading;
using Dapper;
using InventoryManagement.Application.Common.Interfaces.Item.PutAway;
using InventoryManagement.Application.Item.PutAway.Queries.GetAllPutAwayRule;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using InventoryManagement.Application.Item.PutAway.Queries.GetPutAwayRuleItemId;
using InventoryManagement.Application.Common.Interfaces;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using MongoDB.Driver.Core.Configuration; 

namespace InventoryManagement.Infrastructure.Repositories.Item.Templates
{
    public sealed class PutAwayRuleQueryRepository : IPutAwayRuleQueryRepository
    {
        private readonly IDbConnection _db;                 
        private readonly IRackLookup _rackLookup;       
        private readonly IBinLookup _binLookup;    
         private readonly IWarehouseLookup _whLookup;
         private readonly IIPAddressService _ipAddressService;

        public PutAwayRuleQueryRepository(
            IDbConnection db,
            IRackLookup rackLookup,
            IBinLookup binLookup, IWarehouseLookup whLookup,
            IIPAddressService ipAddressService)
        {
            _db = db;
            _rackLookup = rackLookup;
            _binLookup = binLookup;
            _whLookup = whLookup;
            _ipAddressService = ipAddressService;
        }

        public async Task<(IEnumerable<PutAwayRuleListDto> rows, int total)> GetAllAsync(
            int page, int size, string? search, CancellationToken ct = default)
        {
            var skip = (page - 1) * size;
            var sql = """
            ;WITH R AS (
              SELECT 
                r.Id,
                r.UnitId,
                r.ItemGroupId,
                ig.ItemGroupName     AS ItemGroupName,
                r.ItemCategoryId,
                ic.ItemCategoryName  AS ItemCategoryName,
                r.ItemId,
                im.ItemName          AS ItemName,
                r.WarehouseId,r.IsActive
              FROM Inventory.PutAwayRule r
              JOIN Inventory.ItemGroup    ig ON ig.Id = r.ItemGroupId    AND ig.IsDeleted = 0
              JOIN Inventory.ItemCategory ic ON ic.Id = r.ItemCategoryId AND ic.IsDeleted = 0
              LEFT JOIN Inventory.ItemMaster im ON im.Id = r.ItemId       AND im.IsDeleted = 0
              WHERE r.IsDeleted = 0
                AND (
                    @Search IS NULL
                    OR ig.ItemGroupName    LIKE @Like
                    OR ic.ItemCategoryName LIKE @Like
                    OR im.ItemName         LIKE @Like
                )
            )
            SELECT * FROM R
            ORDER BY Id DESC
            OFFSET @skip ROWS FETCH NEXT @size ROWS ONLY;    

            SELECT COUNT(*)
            FROM Inventory.PutAwayRule r
            JOIN Inventory.ItemGroup    ig ON ig.Id = r.ItemGroupId    AND ig.IsDeleted = 0
            JOIN Inventory.ItemCategory ic ON ic.Id = r.ItemCategoryId AND ic.IsDeleted = 0
            LEFT JOIN Inventory.ItemMaster im ON im.Id = r.ItemId       AND im.IsDeleted = 0
            WHERE r.IsDeleted = 0
              AND (
                  @Search IS NULL
                  OR ig.ItemGroupName    LIKE @Like
                  OR ic.ItemCategoryName LIKE @Like
                  OR im.ItemName         LIKE @Like
              );  
            """;

            var like = string.IsNullOrWhiteSpace(search) ? null : $"%{search}%";
            using var m = await _db.QueryMultipleAsync(
                new CommandDefinition(sql, new { skip, size, Search = search, Like = like }, cancellationToken: ct));

            var rows = (await m.ReadAsync<PutAwayRuleListDto>()).ToList();
            var total = await m.ReadSingleAsync<int>();
            
           
            if (rows.Count > 0)
            {
                var whIds = rows.Select(r => r.WarehouseId).Distinct().ToArray();
                var warehouses = await _whLookup.GetByIdsAsync(whIds, ct);
                var dict = warehouses
                    .Where(w => w != null)
                    .ToDictionary(w => w.Id, w => w);

                foreach (var r in rows)
                {
                    if (dict.TryGetValue(r.WarehouseId, out var w))
                    {
                        r.WarehouseCode = w.WarehouseCode;
                        r.WarehouseName = w.WarehouseName;
                    }
                }
            }
            return (rows, total);
        }

        public async Task<PutAwayRuleDetailDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
           var sql = """
            SELECT 
                r.Id,
                r.UnitId,
                r.ItemGroupId,
                ig.ItemGroupName     AS ItemGroupName,    
                r.ItemCategoryId,
                ic.ItemCategoryName  AS ItemCategoryName, 
                r.ItemId,
                im.ItemName          AS ItemName,         
                r.WarehouseId, r.IsActive   
            FROM Inventory.PutAwayRule r
            JOIN Inventory.ItemGroup     ig ON ig.Id = r.ItemGroupId     AND ig.IsDeleted = 0
            JOIN Inventory.ItemCategory  ic ON ic.Id = r.ItemCategoryId  AND ic.IsDeleted = 0
            LEFT JOIN Inventory.ItemMaster im ON im.Id = r.ItemId        AND im.IsDeleted = 0            
            WHERE r.Id = @id AND r.IsDeleted = 0;

            SELECT s.Id, s.PutAwayRuleId, s.StorageTypeId, s.TargetId, s.PriorityId,MM.code PriorityName,s.IsActive   
            FROM Inventory.PutAwayStrategy s
            JOIN Inventory.MiscMaster     MM ON MM.Id = s.priorityId     AND MM.IsDeleted = 0
            WHERE s.PutAwayRuleId = @id AND s.IsDeleted = 0
            ORDER BY s.PriorityId ASC; 
            """;

            using var m = await _db.QueryMultipleAsync(new CommandDefinition(sql, new { id }, cancellationToken: ct));
            var head = await m.ReadSingleOrDefaultAsync<PutAwayRuleDetailDto>();
            if (head == null) return null;
            head.Strategies = (await m.ReadAsync<PutAwayStrategyDto>()).ToList();
            // Warehouse enrichment (single id)
            var wh = (await _whLookup.GetByIdsAsync(new[] { head.WarehouseId }, ct)).FirstOrDefault();
            if (wh != null)
            {
                head.WarehouseCode = wh.WarehouseCode;
                head.WarehouseName = wh.WarehouseName;
            }
            // ---- Enrich targets via gRPC (Rack/Bin) ----
            var missingTypeIds = head.Strategies
                .Where(s => string.IsNullOrWhiteSpace(s.StorageTypeCode))
                .Select(s => s.StorageTypeId)
                .Distinct()
                .ToArray();

            if (missingTypeIds.Length > 0)
            {
                const string typeSql = "SELECT Id, Code FROM Inventory.MiscMaster WHERE Id IN @Ids AND IsDeleted = 0;";
                var types = await _db.QueryAsync<(int Id, string? Code)>(
                    new CommandDefinition(typeSql, new { Ids = missingTypeIds }, cancellationToken: ct));
                var typeDict = types.Where(t => !string.IsNullOrWhiteSpace(t.Code))
                                    .ToDictionary(t => t.Id, t => t.Code!);

                foreach (var s in head.Strategies.Where(x => string.IsNullOrWhiteSpace(x.StorageTypeCode)))
                    if (typeDict.TryGetValue(s.StorageTypeId, out var code)) s.StorageTypeCode = code;
            }

            // ---- Enrich TargetCode/TargetName via gRPC based on storage type of EACH strategy ----
            string Norm(string? s) =>
                string.IsNullOrWhiteSpace(s) ? "" : s.Trim().ToUpperInvariant().Replace(" ", "").Replace("_", "");

            var binIds  = head.Strategies.Where(s => s.TargetId.HasValue && Norm(s.StorageTypeCode) == "BIN")
                                        .Select(s => s.TargetId!.Value).Distinct().ToArray();
            var rackIds = head.Strategies.Where(s => s.TargetId.HasValue && Norm(s.StorageTypeCode) == "RACK")
                                        .Select(s => s.TargetId!.Value).Distinct().ToArray();

            // parallel gRPC fetches
            var binResults = binIds.Any()
                ? await _binLookup.GetByIdsAsync(binIds, ct)
                : Array.Empty<BinLookupDto>();

            var rackResults = rackIds.Any()
                ? await _rackLookup.GetByIdsAsync(rackIds, ct)
                : Array.Empty<RackLookupDto>();

            var binById = binResults.Where(b => b != null).ToDictionary(b => b.Id, b => b);
            var rackById = rackResults.Where(r => r != null).ToDictionary(r => r.Id, r => r);

            foreach (var s in head.Strategies)
            {
                if (!s.TargetId.HasValue) continue;

                var t = Norm(s.StorageTypeCode);
                if (t == "BIN" && binById.TryGetValue(s.TargetId.Value, out var b))
                {
                    s.TargetCode = b.BinCode;
                    s.TargetName = b.BinName;
                }
                else if (t == "RACK" && rackById.TryGetValue(s.TargetId.Value, out var r))
                {
                    s.TargetCode = r.RackCode;
                    s.TargetName = r.RackName;
                }
                // OpenSpace/others: add when those services exist
            }
            return head;
        }

        private static string Normalize(string text) =>
            string.IsNullOrWhiteSpace(text) ? string.Empty : text.Trim().ToUpperInvariant().Replace(" ", "").Replace("_", "");

        private static Contracts.Dtos.Warehouse.BinDto? PickFirstUsableBin(
            List<Contracts.Dtos.Warehouse.BinDto> bins,
            PutAwayEvaluateRequest req)
        {            
            return bins.FirstOrDefault();
        }



       public async Task<List<GetPutAwayRuleItemIdDto>> GetPutAwayRuleDetailsAsync(List<int> itemIds,List<int> warehouseIds)
        {
            if (itemIds == null || !itemIds.Any())
                return new List<GetPutAwayRuleItemIdDto>();

            var unitId = _ipAddressService.GetUnitId();

            var sql = @"
                
                    WITH ItemData AS (
                    SELECT Id AS ItemId, ItemGroupId, ItemCategoryId, ItemCode, StockUomId, ItemName
                    FROM Inventory.ItemMaster
                    WHERE Id IN @ItemIds
                ),
                RankedRules AS (
                    SELECT
                        r.Id AS PutAwayRuleId,
                        r.UnitId,
                        im.ItemId,
                        im.ItemCode,
                        im.StockUomId,
                        ip.PurchaseUomId, -- ✅ Added Purchase UOM
                        im.ItemName,
                        r.ItemId AS RuleItemId,
                        r.ItemCategoryId,
                        r.ItemGroupId,
                        COALESCE(r.WarehouseId, r.WarehouseId) AS WarehouseId,
                        ROW_NUMBER() OVER (
                            PARTITION BY im.ItemId
                            ORDER BY 
                                CASE 
                                    WHEN r.ItemId = im.ItemId THEN 1
                                    WHEN r.ItemGroupId = im.ItemGroupId AND r.ItemCategoryId = im.ItemCategoryId THEN 2
                                    WHEN r.ItemGroupId = im.ItemGroupId OR r.ItemCategoryId = im.ItemCategoryId THEN 3
                                    ELSE 4
                                END ASC
                        ) AS rn
                    FROM Inventory.PutAwayRule r
                    INNER JOIN ItemData im
                        ON r.ItemId = im.ItemId
                        OR (r.ItemId IS NULL AND r.ItemGroupId = im.ItemGroupId AND r.ItemCategoryId = im.ItemCategoryId)
                        OR (r.ItemId IS NULL AND (r.ItemGroupId = im.ItemGroupId OR r.ItemCategoryId = im.ItemCategoryId))
                        OR (r.ItemId IS NULL AND r.ItemGroupId IS NULL AND r.ItemCategoryId IS NULL)
                    LEFT JOIN Inventory.ItemPurchase ip  -- ✅ Join added here
                        ON ip.ItemId = im.ItemId
                    WHERE r.UnitId = @UnitId 
                    AND r.WarehouseId IN @WarehouseIds
                    AND r.IsDeleted = 0 
                    AND r.IsActive = 1
                )
                SELECT 
                    rr.PutAwayRuleId,
                    s.StorageTypeId,
                    MS.Description AS StorageTypeName,
                    s.TargetId,
                    MM.Id AS PriorityId,
                    MM.Description AS PriorityName,
                    rr.WarehouseId,
                    rr.UnitId,
                    rr.ItemId,
                    rr.ItemCode,
                    rr.RuleItemId,
                    rr.ItemCategoryId,
                    ic.ItemCategoryName,
                    rr.ItemGroupId,
                    ig.ItemGroupName,
                    rr.StockUomId,
                    US.UOMName AS StockUom,
                    rr.PurchaseUomId,  
                    UM.UOMName AS PurchaseUom,
                    rr.ItemName,

                    -- ✅ Show ConversionRate only if PurchaseUOM and StockUOM differ
                    CASE 
                        WHEN rr.PurchaseUomId <> rr.StockUomId THEN ITU.ConversionRate
                        ELSE NULL
                    END AS ConversionRate

                FROM RankedRules rr
                INNER JOIN Inventory.PutAwayStrategy s 
                    ON rr.PutAwayRuleId = s.PutAwayRuleId
                LEFT JOIN Inventory.ItemCategory ic 
                    ON rr.ItemCategoryId = ic.Id
                LEFT JOIN Inventory.ItemGroup ig 
                    ON rr.ItemGroupId = ig.Id
                INNER JOIN Inventory.MiscMaster MM
                    ON MM.Id = s.PriorityId
                INNER JOIN Inventory.MiscMaster MS
                    ON MS.Id = s.StorageTypeId
                INNER JOIN Inventory.UOM UM
                    ON rr.PurchaseUomId = UM.Id
                INNER JOIN Inventory.UOM US
                    ON rr.StockUomId = US.Id
                LEFT JOIN Inventory.ItemUOM ITU
                    ON rr.StockUomId = ITU.ConversionUOMId 
                    AND rr.ItemId = ITU.ItemId
                WHERE rr.rn = 1 
                ORDER BY rr.ItemId, s.PriorityId;

            ";

            var rules = (await _db.QueryAsync<GetPutAwayRuleItemIdDto>(sql, new { ItemIds = itemIds, UnitId = unitId,WarehouseIds = warehouseIds })).ToList();

           if (!rules.Any())
            return new List<GetPutAwayRuleItemIdDto>();

            // Enrich with warehouse info
            var ruleWarehouseIds = rules
                .Where(r => r.WarehouseId.HasValue)
                .Select(r => r.WarehouseId!.Value)
                .Distinct()
                .ToList();

            if (ruleWarehouseIds.Any())
            {
                var warehouses = await _whLookup.GetByIdsAsync(ruleWarehouseIds);
                var warehouseDict = warehouses
                    .Where(w => w != null)
                    .ToDictionary(w => w.Id, w => w);

                foreach (var rule in rules)
                {
                    if (rule.WarehouseId.HasValue && warehouseDict.TryGetValue(rule.WarehouseId.Value, out var wh))
                    {
                        rule.WarehouseCode = wh.WarehouseCode;
                        rule.WarehouseName = wh.WarehouseName;
                    }
                }
            }

               // === NEW: Bin / Rack enrichment based on StorageTypeName ===
                    var binIds = rules
                        .Where(r => string.Equals(r.StorageTypeName, "Bin", StringComparison.OrdinalIgnoreCase))
                        .Select(r => r.TargetId)
                        .Where(id => id > 0)
                        .Distinct()
                        .ToList();

                    var rackIds = rules
                        .Where(r => string.Equals(r.StorageTypeName, "Rack", StringComparison.OrdinalIgnoreCase))
                        .Select(r => r.TargetId)
                        .Where(id => id > 0)
                        .Distinct()
                        .ToList();

                     // Fetch all bins & racks via lookups
            var binResults = binIds.Any()
                ? await _binLookup.GetByIdsAsync(binIds)
                : Array.Empty<BinLookupDto>();

            var rackResults = rackIds.Any()
                ? await _rackLookup.GetByIdsAsync(rackIds)
                : Array.Empty<RackLookupDto>();

                    // Handle duplicate Bin/Rack IDs safely
                    var binById = binResults
                        .Where(b => b != null)
                        .GroupBy(b => b!.Id)
                        .ToDictionary(g => g.Key, g => g.First());

                    var rackById = rackResults
                        .Where(r => r != null)
                        .GroupBy(r => r!.Id)
                        .ToDictionary(g => g.Key, g => g.First());

                    // Final enrichment
                    foreach (var r in rules)
                    {
                        if (r.TargetId == 0) continue;

                        if (string.Equals(r.StorageTypeName, "Bin", StringComparison.OrdinalIgnoreCase)
                            && binById.TryGetValue(r.TargetId, out var bin))
                        {
                            r.TargetCode = bin.BinCode;
                            r.TargetName = bin.BinName;
                        }
                        else if (string.Equals(r.StorageTypeName, "Rack", StringComparison.OrdinalIgnoreCase)
                            && rackById.TryGetValue(r.TargetId, out var rack))
                        {
                            r.TargetCode = rack.RackCode;
                            r.TargetName = rack.RackName;
                        }
                    }
            return rules.ToList();
        }

        public async Task<List<GetPutAwayRuleItemIdDto?>> GetPutAwayRuleWarehouseDetailsAsync(List<int> itemids, List<int> warehouseIds)
        {
            if (itemids == null || !itemids.Any())
                return null;

            if (warehouseIds == null || !warehouseIds.Any())
                return null;

            var unitId = _ipAddressService.GetUnitId();

            var sql = @"
                
                    WITH ItemData AS (
                SELECT Id AS ItemId, ItemGroupId, ItemCategoryId, ItemCode, StockUomId, ItemName
                FROM Inventory.ItemMaster
                WHERE Id IN @ItemIds
            ),
            RankedRules AS (
                SELECT
                    r.Id AS PutAwayRuleId,
                    r.UnitId,
                    im.ItemId,
                    im.ItemCode,
                    im.StockUomId,
                    ip.PurchaseUomId,
                    im.ItemName,
                    r.ItemId AS RuleItemId,
                    r.WarehouseId AS WarehouseId,
                    ROW_NUMBER() OVER (
                        PARTITION BY im.ItemId
                        ORDER BY 
                            CASE 
                                WHEN r.ItemId = im.ItemId THEN 1
                                WHEN r.ItemGroupId = im.ItemGroupId AND r.ItemCategoryId = im.ItemCategoryId THEN 2
                                WHEN r.ItemGroupId = im.ItemGroupId OR r.ItemCategoryId = im.ItemCategoryId THEN 3
                                ELSE 4
                            END ASC
                    ) AS rn
                FROM Inventory.PutAwayRule r
                INNER JOIN ItemData im
                    ON r.ItemId = im.ItemId
                    OR (r.ItemId IS NULL AND r.ItemGroupId = im.ItemGroupId AND r.ItemCategoryId = im.ItemCategoryId)
                    OR (r.ItemId IS NULL AND (r.ItemGroupId = im.ItemGroupId OR r.ItemCategoryId = im.ItemCategoryId))
                    OR (r.ItemId IS NULL AND r.ItemGroupId IS NULL AND r.ItemCategoryId IS NULL)
                LEFT JOIN Inventory.ItemPurchase ip ON ip.ItemId = im.ItemId
                WHERE r.UnitId = @UnitId
                AND r.WarehouseId IN @WarehouseIds
                AND r.IsDeleted = 0 
                AND r.IsActive = 1
            )
            SELECT 
                rr.PutAwayRuleId,
                s.StorageTypeId,
                MS.Description AS StorageTypeName,
                s.TargetId,
                s.PriorityId,
                PM.Description AS PriorityName,
                rr.WarehouseId,
                rr.UnitId,
                rr.ItemId,
                rr.ItemCode,
                rr.RuleItemId,  
                rr.ItemName
            FROM RankedRules rr
            INNER JOIN Inventory.PutAwayStrategy s ON rr.PutAwayRuleId = s.PutAwayRuleId
            INNER JOIN Inventory.MiscMaster MS ON MS.Id = s.StorageTypeId
            INNER JOIN Inventory.MiscMaster PM ON PM.Id = s.PriorityId
            WHERE rr.rn = 1
            ORDER BY rr.ItemId, s.PriorityId;
            ";

            
            
            var rules = (await _db.QueryAsync<GetPutAwayRuleItemIdDto>(
                sql, new { ItemIds = itemids, UnitId = unitId, WarehouseIds = warehouseIds }
            )).ToList();

            return rules;
        }
    }
}
