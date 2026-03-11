#nullable disable
using System.Data;
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Queries;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemAutoComplete;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;
using Dapper;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Queries
{
    public sealed class ItemQueryRepository : IItemQueryRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;

        public ItemQueryRepository(IDbConnection dbConnection, ApplicationDbContext db, IIPAddressService ipAddressService)
        {
            _db = db;
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
        }
        public async Task<(List<ItemListDto> Items, int TotalCount)> GetAllAsync(
            int? page, int? size, string search, bool onlyActive,
            int? itemGroupId, int? itemCategoryId, CancellationToken ct = default)
        {
            var q = _db.ItemMaster.AsNoTracking()
                .Where(x => x.IsDeleted == BaseEntity.IsDelete.NotDeleted);

            if (onlyActive)
                q = q.Where(x => x.IsActive == BaseEntity.Status.Active);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                q = q.Where(x => x.ItemCode.Contains(term) || x.ItemName.Contains(term));
            }

            if (itemGroupId.HasValue && itemGroupId.Value > 0)
                q = q.Where(x => x.ItemGroupId == itemGroupId.Value);

            if (itemCategoryId.HasValue && itemCategoryId.Value > 0)
                q = q.Where(x => x.ItemCategoryId == itemCategoryId.Value);

            var total = await q.CountAsync(ct);

            var baseQuery = q.OrderByDescending(x => x.Id)
                .Select(x => new ItemListDto
                {
                    Id = x.Id,
                    ItemCode = x.ItemCode,
                    ItemName = x.ItemName,
                    HasVariants = x.HasVariants,
                    IsStockItem = x.IsStockItem,
                    IsCapitalItem = x.IsCapitalItem,
                    UnitId = x.UnitId ?? 0,
                    ParentItemName = x.ParentItem != null ? x.ParentItem.ItemName : null,
                    ItemGroupName = x.ItemGroup != null ? x.ItemGroup.ItemGroupName : null,
                    ItemCategoryName = x.ItemCategory != null ? x.ItemCategory.ItemCategoryName : null,
                    UOMName = x.UOM != null ? x.UOM.UOMName : null,
                    UOMId = x.StockUomId ?? 0,
                    HsnId = x.HSNId ?? 0,
                    IsActive = (x.IsActive == BaseEntity.Status.Active ? 1 : 0),
                    IssueRuleId = x.IssueRuleId,
                    IssueRule = x.MiscIssueRule != null ? x.MiscIssueRule.Code : null,
                    IsOnSpot = x.IsOnSpot
                });

            var paginate = page.HasValue && size.HasValue && page.Value > 0 && size.Value > 0;

            List<ItemListDto> list;
            if (paginate)
            {
                var skip = (page.Value - 1) * size.Value;
                list = await baseQuery.Skip(skip).Take(size.Value).ToListAsync(ct);
            }
            else
            {
                // No pagination → return all
                list = await baseQuery.ToListAsync(ct);
            }
            return (list, total);
        }

        public async Task<ItemDetailsDto> GetByIdAsync(int id, CancellationToken ct = default)
        {
            // Read the two “configuration” rows once
            var basePath = await _db.MiscTypeMaster
                .Where(t => t.MiscTypeCode == MiscEnumEntity.ItemImagePath && t.IsDeleted == 0)
                .Select(t => t.Description)
                .FirstOrDefaultAsync(ct);

            var folder = await _db.MiscTypeMaster
                .Where(t => t.MiscTypeCode == MiscEnumEntity.ItemImage && t.IsDeleted == 0)
                .Select(t => t.Description)
                .FirstOrDefaultAsync(ct);

            string prefix = string.Empty;
            if (!string.IsNullOrWhiteSpace(basePath) && !string.IsNullOrWhiteSpace(folder))
            {
                var b = basePath.TrimEnd('/', '\\');
                var f = folder.Trim('/', '\\');
                prefix = $"{b}/{f}/";
            }

            var dto = await _db.ItemMaster
                .AsNoTracking()
                .Where(i => i.Id == id && i.IsDeleted == BaseEntity.IsDelete.NotDeleted)
                .Select(i => new ItemDetailsDto
                {
                    // ---------- base ----------
                    Id = i.Id,
                    UnitId = i.UnitId ?? 0,
                    ItemCode = i.ItemCode,
                    ItemName = i.ItemName,
                    HSNId = i.HSNId,
                    HSNCode = i.HSNMaster != null ? i.HSNMaster.HSNCode : null,
                    ItemGroupId = i.ItemGroupId,
                    ItemGroupName = i.ItemGroup != null ? i.ItemGroup.ItemGroupName : null,
                    ItemCategoryId = i.ItemCategoryId,
                    ItemCategoryName = i.ItemCategory != null ? i.ItemCategory.ItemCategoryName : null,
                    StockUomId = i.StockUomId,
                    StockUOM = i.UOM != null ? i.UOM.UOMName : null,
                    ItemClassificationId = i.ItemClassificationId,
                    ItemClassification = i.MiscClassification != null ? i.MiscClassification.Code : null,
                    Description = i.Description,
                    ValidFrom = i.ValidFrom,
                    XPlantMaterialStatusId = i.XPlantMaterialStatusId,
                    XPlantMaterialStatus = i.MiscStatus != null ? i.MiscStatus.Code : null,
                    IsStockItem = i.IsStockItem,
                    MaintainStock = i.MaintainStock,
                    HasVariants = i.HasVariants,
                    ParentItemId = i.ParentItemId,
                    ParentItemName = i.ParentItem != null ? i.ParentItem.ItemName : null,
                    ItemImage = i.ItemImage,
                    ItemImageUrl = !string.IsNullOrWhiteSpace(prefix) && !string.IsNullOrWhiteSpace(i.ItemImage)
                        ? prefix + i.ItemImage
                        : null,                        
                    IssueRuleId = i.IssueRuleId??0,
                    IssueRule = i.MiscIssueRule != null ? i.MiscIssueRule.Code : null,
                    IsOnSpot = i.IsOnSpot,
                    GSTPercentage = _db.HSNMaster
                        .Where(h => h.Id == i.HSNId)
                        .Select(h => h.GSTPercentage)
                        .FirstOrDefault(),
                    IsActive = (i.IsActive == BaseEntity.Status.Active ? 1 : 0),

                    // ---------- tabs (1-1) ----------
                    // ItemMaster-level fields (moved from Purchase)
                    OriginCountryId = i.OriginCountryId,
                    TariffNumber = i.TariffNumber,

                    Purchase = i.Purchase == null ? null : new ItemPurchaseDto
                    {
                        PurchaseUomId = i.Purchase.PurchaseUomId,
                        PurchaseUOM = i.Purchase.PurchaseUOM != null ? i.Purchase.PurchaseUOM.UOMName : null,
                        LeadTimeDays = i.Purchase.LeadTimeDays,
                        GrProcessingTimeDays = i.Purchase.GrProcessingTimeDays,
                        AutomaticPo = i.Purchase.AutomaticPo,
                        SourceOfItem = i.Purchase.SourceOfItem
                    },
                    Inventory = i.Inventory == null ? null : new ItemInventoryDto
                    {
                        InventoryUOM = i.Inventory.WeightUOM != null ? i.Inventory.WeightUOM.UOMName : null,
                        DefaultMaterialRequestType = i.Inventory.MiscDefaultMaterialRequestType != null
                            ? i.Inventory.MiscDefaultMaterialRequestType.Code : null,
                        ValuationMethod = i.Inventory.MiscValuationMethod != null
                            ? i.Inventory.MiscValuationMethod.Code : null,
                        RequestType = i.Inventory.MiscRequestType != null
                            ? i.Inventory.MiscRequestType.Code : null,

                        Weight = i.Inventory.Weight,
                        WeightUomId = i.Inventory.WeightUomId,
                        DefaultMaterialRequestTypeId = i.Inventory.DefaultMaterialRequestTypeId,
                        ValuationMethodId = i.Inventory.ValuationMethodId,
                        ShelfLife = i.Inventory.ShelfLife,
                        UpperTolerance = i.Inventory.UpperTolerance,
                        LowerTolerance = i.Inventory.LowerTolerance,
                        BatchNumberSeries = i.Inventory.BatchNumberSeries,
                        SerialNumberSeries = i.Inventory.SerialNumberSeries,
                        ReorderLevel = i.Inventory.ReorderLevel,
                        ReorderQty = i.Inventory.ReorderQty,
                        RequestTypeId = i.Inventory.RequestTypeId,
                        SafetyStock = i.Inventory.SafetyStock,
                        AllowNegativeStock = i.Inventory.AllowNegativeStock,
                        BatchManagement = i.Inventory.BatchManagement,
                        ApplyBatchNumber = i.Inventory.ApplyBatchNumber
                    },
                    Quality = i.Quality == null ? null : new ItemQualityDto
                    {
                        InspectionTemplateId = i.Quality.InspectionTemplateId,
                        CertificateTypeId = i.Quality.CertificateTypeId,
                        InspLotProcessingTime = i.Quality.InspLotProcessingTime,
                        InspectionRequired = i.Quality.InspectionRequired,
                        QualityInspectionFree = i.Quality.QualityInspectionFree,
                        IsCertificateRequiredFromSupplier = i.Quality.IsCertificateRequiredFromSupplier,

                        // Prefer a real nav if you have it, e.g. i.Quality.MiscCertificateType.Code
                        CertificateType = i.Quality.CertificateTypeId != null
                            ? _db.MiscMaster.Where(mm => mm.Id == i.Quality.CertificateTypeId).Select(mm => mm.Code).FirstOrDefault()
                            : null
                    },
                    Sale = i.Sale == null ? null : new ItemSaleDto
                    {
                        Id = i.Sale.Id,
                        UomId = i.Sale.UomId,
                        MinQuantity = i.Sale.MinQuantity,
                        PackageQuantity = i.Sale.PackageQuantity,
                        DeliveryLeadTime = i.Sale.DeliveryLeadTime,
                        Discount = i.Sale.Discount,
                        SalesUOM = i.Sale.SalesUOM != null ? i.Sale.SalesUOM.UOMName : null
                    },

                    // ---------- collections ----------
                    Suppliers = i.Suppliers
                        .OrderBy(s => s.SupplierId)
                        .Select(s => new ItemSupplierDto
                        {
                            SupplierId = s.SupplierId,
                            UnitId = s.UnitId,
                            SupplierPartNo = s.SupplierPartNo,
                            MOQ = s.MOQ,
                            MOQUomId = s.MOQUomId,
                            PackageValue = s.PackageValue,
                            PackageUomId = s.PackageUomId,
                            LeadTime = s.LeadTime,
                            DefaultSupplier = s.DefaultSupplier
                        })
                        .ToList(),

                    Manufacture = i.Manufacture
                        .OrderBy(m => m.UnitId)
                        .Select(m => new ItemManufactureDto
                        {
                            UnitId = m.UnitId,
                            ManufacturingTypeId = m.ManufacturingTypeId,
                            ManufacturingType = _db.MiscMaster
                                .Where(mm => mm.Id == m.ManufacturingTypeId)
                                .Select(mm => mm.Code)
                                .FirstOrDefault()
                        })
                        .ToList(),

                    Uoms = i.ItemUOMs
                        .OrderBy(u => u.Id)
                        .Select(u => new ItemUomDto
                        {
                            ConversionUOMId = u.ConversionUOMId,
                            ConversionRate = u.ConversionRate,
                            ConversionUOM = u.ConversionUOM != null ? u.ConversionUOM.UOMName : null
                        })
                        .ToList(),

                    // ---------- VARIANTS ----------
                    VariantAttributes = _db.Set<ItemVariantAttribute>()
                        .Where(a => a.ItemId == i.Id)
                        .OrderBy(a => a.Order)
                        .Select(a => new VariantAttributeDto
                        {
                            Id = a.Id,
                            AttributeId = a.AttributeId,
                            VariantBasedOn = a.VariantBasedOn,
                            AttributeGroupId = a.AttributeGroupId,
                            Order = a.Order,
                            // If you have a nav to Attribute master with name:
                            AttributeName = _db.MiscMaster
                                .Where(m => m.Id == a.AttributeId)
                                .Select(m => m.Code)              // or m.Description
                                .FirstOrDefault()
                            //AttributeName = null
                        })
                        .ToList(),

                    VariantValues = _db.Set<ItemVariantValue>()
                        .Where(v => v.ItemId == i.Id)
                        .Select(v => new VariantValueDto
                        {
                            VariantAttributeId = v.VariantAttributeId,
                            OptionValue = v.OptionValue,
                            Combo = null
                        })
                        .ToList(),
                })
                .FirstOrDefaultAsync(ct);

            return dto;
        }


        public async Task<string> GetBaseDirectoryAsync()
        {
            const string query = @"
                SELECT Description AS BaseDirectory
                FROM Inventory.MiscTypeMaster
                WHERE MiscTypeCode = 'ItemImage' AND IsDeleted = 0";
            var result = await _dbConnection.QueryFirstOrDefaultAsync<string>(query);
            return result;
        }

        public async Task<bool> RemoveImageReferenceAsync(string imageName, CancellationToken ct = default)
        {
            var asset = await _db.ItemMaster.FirstOrDefaultAsync(x => x.ItemImage == imageName);
            if (asset == null) return false;
            asset.ItemImage = null;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateItemImageAsync(int ItemId, string imageName, CancellationToken ct = default)
        {
            var asset = await _db.ItemMaster.FindAsync(ItemId);
            if (asset == null) return false;
            asset.ItemImage = imageName;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<string> GetLatestItemCode(int itemGroupId, int itemCategoryId, CancellationToken ct = default)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@GroupId", itemGroupId);
            parameters.Add("@CategoryId", itemCategoryId);

            var newAssetCode = await _dbConnection.QueryFirstOrDefaultAsync<string>(
                "dbo.Sp_GetItemCode", parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 120);
            return newAssetCode;
        }

        public async Task<List<string>> GetCandidateItemNamesAsync(string normalizedPrefix, int take = 200, CancellationToken ct = default)
        {
            var hint = normalizedPrefix?.Replace("%", "").Replace("_", "") ?? "";
            return await _db.ItemMaster
                .AsNoTracking()
                .Where(i => i.IsDeleted == BaseEntity.IsDelete.NotDeleted && i.ItemName != null)
                .Where(i => EF.Functions.Like(
                    EF.Functions.Collate(i.ItemName!, "Latin1_General_CI_AI"), $"%{hint}%"))
                .OrderByDescending(i => i.Id)
                .Select(i => i.ItemName!)
                .Take(take <= 0 ? 100 : take)
                .ToListAsync(ct);
        }

        public async Task<List<GetItemAutoCompleteDto>> GetItemAutoCompleteAsync(string searchPattern,int? itemGroupId, int? itemCategoryId,int? sourceId,int? issueRuleId, CancellationToken ct = default)
        {
            var UnitId = _ipAddressService.GetUnitId();
            searchPattern = searchPattern ?? string.Empty;
            const string query = @"
                SELECT IM.Id, ItemName, ItemCode, ParentItemId,HSNId,HSNCode,GSTPercentage,IM.ItemCategoryId,IM.ItemGroupId,IM.TariffNumber,
                P.PurchaseUomId,IM.StockUomId,U.Code as PurchaseUom,U1.Code as StockUom,ISNULL(ST.CurrentStockQty, 0) AS CurrentStockQty
                FROM Inventory.ItemMaster IM
                Inner join Inventory.HSNMaster HM on IM.HSNId=HM.id
               	left join Inventory.ItemPurchase P on P.ItemId=IM.Id
                left join Inventory.UOM U on U.Id=P.PurchaseUomId
                left join Inventory.UOM U1 on U1.Id=IM.StockUomId
                LEFT JOIN (
                        SELECT 
                            SL.ItemId,
                            SUM(SL.ReceivedQty - SL.IssueQty) AS CurrentStockQty
                        FROM Inventory.StockLedger SL
                        WHERE SL.UnitId = @UnitId
                        GROUP BY SL.ItemId
                    ) ST ON ST.ItemId = IM.Id
                WHERE IM.IsDeleted = 0 and IM.IsActive = 1
                AND HasVariants = 0     AND (@GroupId    IS NULL OR @GroupId    <= 0 OR IM.ItemGroupId    = @GroupId)
                AND (@CategoryId IS NULL OR @CategoryId <= 0 OR IM.ItemCategoryId = @CategoryId)                
                AND (@SourceId IS NULL OR @SourceId <= 0 OR P.SourceOfItem = @SourceId OR P.SourceOfItem IS NULL )
                AND (@IssueRuleId IS NULL OR @IssueRuleId <= 0 OR IM.IssueRuleId = @IssueRuleId)
                AND (@Search = '' OR IM.ItemName LIKE @Like OR IM.ItemCode LIKE @Like) ";            
            var parameters = new
            {
                GroupId = itemGroupId,
                CategoryId = itemCategoryId,
                Search = searchPattern,
                Like = $"%{searchPattern}%",
                SourceId = sourceId,
                IssueRuleId = issueRuleId,
                UnitId=UnitId
            };
            var items = await _dbConnection.QueryAsync<GetItemAutoCompleteDto>(query, parameters);
            return items.ToList();
        }

        public async Task<List<GetItemAutoCompleteDto>> GetItemsMasterByIdsAsync(IEnumerable<int> ids)
        {
            var list = ids?.Distinct().ToList() ?? new();
            if (list.Count == 0) return new();

            const string sql = @"
                SELECT 
                    IM.Id               AS Id,
                    IM.ItemName         AS ItemName,
                    IM.ItemCode         AS ItemCode,
                    H.Id                AS HSNId,
                    H.HSNCode           AS HSNCode,
                    H.GSTPercentage     AS GSTPercentage,
                    IM.ItemCategoryId,
                    IM.ItemGroupId,
                    IM.TariffNumber,
                    IM.IsOnSpot,
                    P.PurchaseUomId,
                    IM.StockUomId,
                    P.SourceOfItem,

                    -- Supplier columns
                    ISupp.Id            AS SupplierRowId,
                    ISupp.ItemId        AS SupplierItemId,
                    ISupp.SupplierId,
                    ISupp.UnitId,
                    ISupp.SupplierPartNo,
                    ISupp.DefaultSupplier,
                    ISupp.LeadTime,
                    ISupp.MOQ,
                    ISupp.MOQUomId,
                    ISupp.PackageUomId,
                    ISupp.PackageValue
                FROM Inventory.ItemMaster AS IM
                INNER JOIN Inventory.HSNMaster AS H 
                    ON H.Id = IM.HSNId
                LEFT JOIN Inventory.ItemPurchase AS P 
                    ON P.ItemId = IM.Id
                LEFT JOIN Inventory.ItemSupplier AS ISupp
                    ON ISupp.ItemId = IM.Id
                WHERE IM.IsDeleted = 0
                AND IM.HasVariants = 0
                AND IM.Id IN @Ids;";

            var lookup = new Dictionary<int, GetItemAutoCompleteDto>();

            var result = await _dbConnection.QueryAsync<
                GetItemAutoCompleteDto,
                ItemVendorDto,
                GetItemAutoCompleteDto>(
                sql,
                (item, supplier) =>
                {
                    if (!lookup.TryGetValue(item.Id, out var existing))
                    {
                        existing = item;
                        // ⚠️ Do NOT initialize Vendors here
                        lookup.Add(existing.Id, existing);
                    }

                    // When no supplier row, Dapper gives default ItemVendorDto (SupplierId = 0)
                    if (supplier != null && supplier.SupplierId != 0)
                    {
                        if (existing.Vendors == null)
                            existing.Vendors = new List<ItemVendorDto>();

                        existing.Vendors.Add(supplier);
                    }

                    return existing;
                },
                new { Ids = list },
                splitOn: "SupplierRowId"
            );

            return lookup.Values.ToList();
        }


        public async Task<List<ItemPurchaseToleranceDto>> GetItemPurchaseToleranceAsync(
            IEnumerable<int> itemIds,
            CancellationToken ct = default)
        {
                var ids = itemIds?.Where(i => i > 0).Distinct().ToArray();
                if (ids == null || ids.Length == 0) return new();
            const string sql = @"
                SELECT
                    b.Id            AS ItemId,
                    b.ItemCode,
                    b.ItemName,
                    a.LowerTolerance,
                    a.UpperTolerance,
                    ip.PurchaseUomId,
                    u.UOMName
                FROM Inventory.ItemMaster AS b
                LEFT JOIN Inventory.ItemInventory AS a
                    ON a.ItemId = b.Id
                OUTER APPLY (
                    SELECT TOP (1) *
                    FROM Inventory.ItemPurchase p
                    WHERE p.ItemId = b.Id
                    ORDER BY p.Id DESC
                ) AS ip
                LEFT JOIN Inventory.UOM AS u
                    ON u.Id = ip.PurchaseUomId
                WHERE b.Id IN @ItemIds
                ORDER BY b.ItemCode;";
            var cmd = new CommandDefinition(sql, new { ItemIds = ids }, cancellationToken: ct);
                    var rows = await _dbConnection.QueryAsync<ItemPurchaseToleranceDto>(cmd);
                    return rows.ToList();
        }

        public async Task<List<GetItemAutoCompleteDto>> GetItemsByVariantFilterAsync(
            bool? hasVariant, int? parentItemId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT IM.Id, IM.ItemName, IM.ItemCode, IM.ParentItemId,
                       IM.HSNId, HM.HSNCode, HM.GSTPercentage,
                       IM.ItemCategoryId, IM.ItemGroupId,
                       IM.TariffNumber, P.PurchaseUomId, IM.StockUomId,
                       U.Code AS PurchaseUom, U1.Code AS StockUom, IM.IsOnSpot
                FROM Inventory.ItemMaster IM
                INNER JOIN Inventory.HSNMaster HM ON IM.HSNId = HM.Id
                LEFT JOIN Inventory.ItemPurchase P ON P.ItemId = IM.Id
                LEFT JOIN Inventory.UOM U ON U.Id = P.PurchaseUomId
                LEFT JOIN Inventory.UOM U1 ON U1.Id = IM.StockUomId
                WHERE IM.IsDeleted = 0 AND IM.IsActive = 1
                AND (@HasVariant IS NULL OR IM.HasVariants = @HasVariant)
                AND (@ParentItemId IS NULL OR IM.ParentItemId = @ParentItemId)";

            var parameters = new { HasVariant = hasVariant, ParentItemId = parentItemId };
            var items = await _dbConnection.QueryAsync<GetItemAutoCompleteDto>(sql, parameters);
            return items.ToList();
        }

    }
}
