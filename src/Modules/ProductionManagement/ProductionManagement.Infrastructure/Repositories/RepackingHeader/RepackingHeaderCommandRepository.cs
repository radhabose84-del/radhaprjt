using Contracts.Dtos.Stock;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProductionManagement.Application.Common.Interfaces.IRepackingHeader;
using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;
using ProductionManagement.Infrastructure.Data;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Infrastructure.Repositories.RepackingHeader
{
    public class RepackingHeaderCommandRepository : IRepackingHeaderCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly ISalesMiscMasterLookup _salesMiscMasterLookup;
        private readonly ISalesStockLedgerService _salesStockLedgerService;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;

        public RepackingHeaderCommandRepository(
            ApplicationDbContext applicationDbContext,
            ISalesMiscMasterLookup salesMiscMasterLookup,
            ISalesStockLedgerService salesStockLedgerService,
            IDocumentSequenceLookup documentSequenceLookup)
        {
            _applicationDbContext = applicationDbContext;
            _salesMiscMasterLookup = salesMiscMasterLookup;
            _salesStockLedgerService = salesStockLedgerService;
            _documentSequenceLookup = documentSequenceLookup;
        }

        public async Task<int> CreateAsync(Domain.Entities.RepackingHeader entity, int typeId)
        {
            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    // Get status IDs
                    var packedStatus = await _salesMiscMasterLookup.GetByCodeAsync(MiscEnumEntity.Packed);
                    var packedStatusId = packedStatus?.Id ?? 0;

                    var deletedStatus = await _salesMiscMasterLookup.GetByCodeAsync(MiscEnumEntity.Deleted);
                    var deletedStatusId = deletedStatus?.Id ?? 0;

                    // Determine type based on ItemId == OldItemId
                    bool isRepacking = entity.ItemId == entity.OldItemId;
                    var typeCode = isRepacking ? "Repacking" : "YarnConversion";

                    // TypeId for Sales.StockLedger entries → Sales.MiscMaster
                    var salesStockType = await _salesMiscMasterLookup.GetByCodeAsync(typeCode);
                    var salesStockTypeId = salesStockType?.Id;

                    // Fetch source info per detail (provides OldTotalBags for pack-no calc)
                    var sourceInfoMap = new Dictionary<int, StockPackSourceDto?>();
                    if (entity.RepackingDetails != null)
                    {
                        foreach (var detail in entity.RepackingDetails)
                        {
                            sourceInfoMap[detail.OldStartPackNo] = await _salesStockLedgerService.GetPackSourceInfoAsync(
                                detail.OldStartPackNo, detail.OldEndPackNo,
                                entity.ProductionYear, entity.UnitId);
                        }
                    }

                    // Auto-calculate StartPackNo/EndPackNo for each detail
                    if (isRepacking)
                    {
                        // Repacking: distribute new pack range (header.StartPackNo–EndPackNo) across details.
                        // For a single detail the entire header range is assigned directly.
                        // For multiple details packs are distributed proportionally by old bag count,
                        // with the last detail anchored to header.EndPackNo to absorb any rounding.
                        int totalNewBags = entity.EndPackNo - entity.StartPackNo + 1;
                        int totalOldBags = entity.RepackingDetails?.Sum(d =>
                            sourceInfoMap.GetValueOrDefault(d.OldStartPackNo)?.OldTotalBags ?? 0) ?? 0;

                        if (entity.RepackingDetails != null)
                        {
                            int nextPackNo = entity.StartPackNo;
                            var detailList = entity.RepackingDetails.ToList();
                            for (int idx = 0; idx < detailList.Count; idx++)
                            {
                                var detail = detailList[idx];
                                bool isLast = idx == detailList.Count - 1;
                                detail.StartPackNo = nextPackNo;
                                if (isLast)
                                {
                                    detail.EndPackNo = entity.EndPackNo;
                                }
                                else
                                {
                                    var oldBags = sourceInfoMap.GetValueOrDefault(detail.OldStartPackNo)?.OldTotalBags ?? 0;
                                    int newBags = totalOldBags > 0
                                        ? (int)Math.Round((double)oldBags / totalOldBags * totalNewBags)
                                        : 0;
                                    detail.EndPackNo = nextPackNo + newBags - 1;
                                    nextPackNo = detail.EndPackNo + 1;
                                }
                            }
                        }
                    }
                    else
                    {
                        // YarnConversion: keep same pack numbers — source packs deleted, new items inserted at same PackNos
                        if (entity.RepackingDetails != null)
                        {
                            foreach (var detail in entity.RepackingDetails)
                            {
                                detail.StartPackNo = detail.OldStartPackNo;
                                detail.EndPackNo = detail.OldEndPackNo;
                            }
                        }
                    }

                    // Save header + details
                    await _applicationDbContext.RepackingHeader.AddAsync(entity);
                    await _applicationDbContext.SaveChangesAsync();

                    // Mark source packs as Deleted for each detail row
                    if (entity.RepackingDetails != null)
                    {
                        foreach (var detail in entity.RepackingDetails)
                        {
                            await _salesStockLedgerService.UpdateStatusByPackRangeAsync(
                                "PROD",
                                detail.OldStartPackNo, detail.OldEndPackNo,
                                packedStatusId, deletedStatusId,
                                entity.ProductionYear, entity.UnitId);
                        }
                    }

                    // Inherit LotId from source packs.
                    // lotId      = LotId to assign to new packs in Sales.StockLedger
                    // sourceLotId = LotId of the source packs (used as old-item ledger key)
                    var lotId      = entity.LotId;
                    var sourceLotId = entity.LotId;
                    if (entity.RepackingDetails?.Count > 0)
                    {
                        var firstDetail = entity.RepackingDetails.First();
                        var packLotId = await _salesStockLedgerService.GetLotIdByPackRangeAsync(
                            firstDetail.OldStartPackNo, firstDetail.OldEndPackNo,
                            entity.ProductionYear, entity.UnitId);
                        sourceLotId = packLotId;
                        if (isRepacking)
                            lotId = packLotId; // repacking: new packs inherit source lot
                        // YarnConversion: new packs keep entity.LotId; source stays as sourceLotId
                    }

                    // Insert new target packs per detail row (DetailDocNo = RepackingDetail.Id)
                    var stockEntries = new List<SalesStockLedgerDto>();
                    if (entity.RepackingDetails != null)
                    {
                        foreach (var detail in entity.RepackingDetails)
                        {
                            int currentPackNo = detail.StartPackNo;
                            int packCount = detail.EndPackNo - detail.StartPackNo + 1;
                            for (int i = 0; i < packCount; i++)
                            {
                                stockEntries.Add(new SalesStockLedgerDto
                                {
                                    UnitId = entity.UnitId,
                                    DocType = "PROD",
                                    DocNo = entity.Id,
                                    DetailDocNo = detail.Id,
                                    DocDate = entity.RepackDate.ToDateTime(TimeOnly.MinValue),
                                    ItemId = entity.ItemId,
                                    LotId = lotId,
                                    PackNo = currentPackNo++,
                                    PackTypeId = entity.PackTypeId,
                                    WarehouseId = entity.WarehouseId,
                                    BinId = entity.BinId,
                                    TotalQty = 1,
                                    TotalValue = entity.NetWeightPerPack,
                                    StatusId = packedStatusId,
                                    TypeId = salesStockTypeId
                                });
                            }
                        }
                    }

                    await _salesStockLedgerService.InsertAsync(stockEntries);

                    // Upsert ProductionStockLedger — one row per (UnitId, ItemId, LotId, DocDate)
                    if (isRepacking)
                    {
                        // REPACKING (same item): update/create row for OldItemId
                        var existingLedger = await _applicationDbContext.ProductionStockLedger
                            .FirstOrDefaultAsync(l => l.UnitId == entity.UnitId
                                && l.ItemId == entity.OldItemId && l.LotId == sourceLotId
                                && l.DocDate == entity.RepackDate);

                        if (existingLedger != null)
                        {
                            existingLedger.BagsRepacked     = entity.TotalBags;
                            existingLedger.RepackKgs        = entity.NetWeight;
                            // ClosingLooseKgs = PACK loose + REPACK loose (ADD, not overwrite)
                            existingLedger.ClosingLooseKgs += entity.LooseConeKgs;
                        }
                        else
                        {
                            await _applicationDbContext.ProductionStockLedger.AddAsync(new ProductionStockLedger
                            {
                                UnitId           = entity.UnitId,
                                ItemId           = entity.OldItemId,
                                LotId            = sourceLotId,
                                DocDate          = entity.RepackDate,
                                OpeningLooseKgs  = 0,
                                ProdKgs          = 0,
                                TotalProdKgs     = 0,
                                PackTypeId       = entity.PackTypeId,
                                NetWeightPerPack = entity.NetWeightPerPack,
                                TotalBags        = 0,
                                NetWeight        = 0,
                                BagsRepacked     = entity.TotalBags,
                                RepackKgs        = entity.NetWeight,
                                ClosingLooseKgs  = entity.LooseConeKgs,
                                ClosingPackKgs   = 0,
                                ClosingBags      = 0
                            });
                        }

                        // Mark old-item previous entries as stock-closed
                        var prevEntries = await _applicationDbContext.ProductionStockLedger
                            .Where(l => l.UnitId == entity.UnitId && l.ItemId == entity.OldItemId
                                && l.LotId == sourceLotId && l.DocDate < entity.RepackDate && !l.StockClosing)
                            .ToListAsync();
                        foreach (var prev in prevEntries)
                            prev.StockClosing = true;
                    }
                    else
                    {
                        // YARN CONVERSION (OldItemId != ItemId): two separate ledger rows.

                        // ── 1. OLD item row — record conversion-out, reduce closing stock ──
                        var prevOldLedger = await _applicationDbContext.ProductionStockLedger
                            .Where(l => l.UnitId == entity.UnitId && l.ItemId == entity.OldItemId
                                && l.LotId == sourceLotId && l.DocDate < entity.RepackDate)
                            .OrderByDescending(l => l.DocDate).ThenByDescending(l => l.Id)
                            .FirstOrDefaultAsync();
                        var prevOldClosingPackKgs = prevOldLedger?.ClosingPackKgs ?? 0;
                        var prevOldClosingBags    = prevOldLedger?.ClosingBags ?? 0;

                        var oldItemLedger = await _applicationDbContext.ProductionStockLedger
                            .FirstOrDefaultAsync(l => l.UnitId == entity.UnitId
                                && l.ItemId == entity.OldItemId && l.LotId == sourceLotId
                                && l.DocDate == entity.RepackDate);

                        if (oldItemLedger != null)
                        {
                            // Row already exists (production also happened today for old item)
                            // YarnConversion: do NOT touch BagsRepacked/RepackKgs on old item row
                            // (those belong to any repacking that may have set them for this date)
                            oldItemLedger.ClosingPackKgs  -= entity.NetWeight;
                            oldItemLedger.ClosingBags     -= entity.TotalBags;
                            oldItemLedger.ClosingLooseKgs += entity.LooseConeKgs;
                        }
                        else
                        {
                            await _applicationDbContext.ProductionStockLedger.AddAsync(new ProductionStockLedger
                            {
                                UnitId           = entity.UnitId,
                                ItemId           = entity.OldItemId,
                                LotId            = sourceLotId,
                                DocDate          = entity.RepackDate,
                                OpeningLooseKgs  = 0,
                                ProdKgs          = 0,
                                TotalProdKgs     = 0,
                                PackTypeId       = entity.OldPackTypeId,
                                NetWeightPerPack = entity.NetWeightPerPack,
                                TotalBags        = 0,
                                NetWeight        = 0,
                                BagsRepacked     = 0,
                                RepackKgs        = 0,
                                ClosingLooseKgs  = entity.LooseConeKgs,
                                ClosingPackKgs   = prevOldClosingPackKgs - entity.NetWeight,
                                ClosingBags      = prevOldClosingBags - entity.TotalBags
                            });
                        }

                        // Mark old-item previous entries as stock-closed
                        var prevOldEntries = await _applicationDbContext.ProductionStockLedger
                            .Where(l => l.UnitId == entity.UnitId && l.ItemId == entity.OldItemId
                                && l.LotId == sourceLotId && l.DocDate < entity.RepackDate && !l.StockClosing)
                            .ToListAsync();
                        foreach (var prev in prevOldEntries)
                            prev.StockClosing = true;

                        // ── 2. NEW item row — record conversion-in, add to closing stock ──
                        var prevNewLedger = await _applicationDbContext.ProductionStockLedger
                            .Where(l => l.UnitId == entity.UnitId && l.ItemId == entity.ItemId
                                && l.LotId == entity.LotId && l.DocDate < entity.RepackDate)
                            .OrderByDescending(l => l.DocDate).ThenByDescending(l => l.Id)
                            .FirstOrDefaultAsync();
                        var prevNewClosingPackKgs = prevNewLedger?.ClosingPackKgs ?? 0;
                        var prevNewClosingBags    = prevNewLedger?.ClosingBags ?? 0;

                        var newItemLedger = await _applicationDbContext.ProductionStockLedger
                            .FirstOrDefaultAsync(l => l.UnitId == entity.UnitId
                                && l.ItemId == entity.ItemId && l.LotId == entity.LotId
                                && l.DocDate == entity.RepackDate);

                        if (newItemLedger != null)
                        {
                            // Row already exists (production also happened today for new item)
                            // YarnConversion: BagsRepacked/RepackKgs record conversion-in quantities
                            newItemLedger.BagsRepacked   += entity.TotalBags;
                            newItemLedger.RepackKgs      += entity.NetWeight;
                            newItemLedger.ClosingPackKgs += entity.NetWeight;
                            newItemLedger.ClosingBags    += entity.TotalBags;
                        }
                        else
                        {
                            await _applicationDbContext.ProductionStockLedger.AddAsync(new ProductionStockLedger
                            {
                                UnitId           = entity.UnitId,
                                ItemId           = entity.ItemId,
                                LotId            = entity.LotId,
                                DocDate          = entity.RepackDate,
                                OpeningLooseKgs  = 0,
                                ProdKgs          = 0,
                                TotalProdKgs     = 0,
                                PackTypeId       = entity.PackTypeId,
                                NetWeightPerPack = entity.NetWeightPerPack,
                                TotalBags        = 0,
                                NetWeight        = 0,
                                BagsRepacked     = entity.TotalBags,
                                RepackKgs        = entity.NetWeight,
                                ClosingLooseKgs  = 0,
                                ClosingPackKgs   = prevNewClosingPackKgs + entity.NetWeight,
                                ClosingBags      = prevNewClosingBags + entity.TotalBags
                            });
                        }

                        // Mark new-item previous entries as stock-closed
                        var prevNewEntries = await _applicationDbContext.ProductionStockLedger
                            .Where(l => l.UnitId == entity.UnitId && l.ItemId == entity.ItemId
                                && l.LotId == entity.LotId && l.DocDate < entity.RepackDate && !l.StockClosing)
                            .ToListAsync();
                        foreach (var prev in prevNewEntries)
                            prev.StockClosing = true;
                    }

                    await _applicationDbContext.SaveChangesAsync();

                    // Increment document sequence
                    var dbConnection = _applicationDbContext.Database.GetDbConnection();
                    var dbTransaction = transaction.GetDbTransaction();
                    await _documentSequenceLookup.IncrementDocNoAsync(typeId, dbConnection, dbTransaction);

                    await transaction.CommitAsync();
                    return entity.Id;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<int> UpdateAsync(Domain.Entities.RepackingHeader entity)
        {
            var existingEntity = await _applicationDbContext.RepackingHeader
                .Include(h => h.RepackingDetails)
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    var packedStatus = await _salesMiscMasterLookup.GetByCodeAsync(MiscEnumEntity.Packed);
                    var packedStatusId = packedStatus?.Id ?? 0;

                    var deletedStatus = await _salesMiscMasterLookup.GetByCodeAsync(MiscEnumEntity.Deleted);
                    var deletedStatusId = deletedStatus?.Id ?? 0;

                    bool isRepacking = entity.ItemId == entity.OldItemId;
                    var typeCode = isRepacking ? "Repacking" : "YarnConversion";

                    // TypeId for Sales.StockLedger entries → Sales.MiscMaster
                    var salesStockType = await _salesMiscMasterLookup.GetByCodeAsync(typeCode);
                    var salesStockTypeId = salesStockType?.Id;

                    // Restore old source packs back to Packed (from existing details)
                    if (existingEntity.RepackingDetails != null)
                    {
                        foreach (var detail in existingEntity.RepackingDetails)
                        {
                            await _salesStockLedgerService.UpdateStatusByPackRangeAsync(
                                "PROD",
                                detail.OldStartPackNo, detail.OldEndPackNo,
                                deletedStatusId, packedStatusId,
                                existingEntity.ProductionYear, existingEntity.UnitId);
                        }
                    }

                    // Delete old StockLedger entries for this doc
                    await _salesStockLedgerService.DeleteByDocAsync(
                        "PROD", existingEntity.Id, existingEntity.ProductionYear, existingEntity.UnitId);

                    // Mark new source packs as Deleted (from incoming details)
                    if (entity.RepackingDetails != null)
                    {
                        foreach (var detail in entity.RepackingDetails)
                        {
                            await _salesStockLedgerService.UpdateStatusByPackRangeAsync(
                                "PROD",
                                detail.OldStartPackNo, detail.OldEndPackNo,
                                packedStatusId, deletedStatusId,
                                entity.ProductionYear, entity.UnitId);
                        }
                    }

                    // Save old key and old REPACK loose for ledger cleanup
                    var oldOldItemId = existingEntity.OldItemId;
                    var oldRepackDate = existingEntity.RepackDate;
                    var oldLooseConeKgs = existingEntity.LooseConeKgs;

                    // Update header fields (StartPackNo/EndPackNo come from payload)
                    existingEntity.RepackDate = entity.RepackDate;
                    existingEntity.ItemId = entity.ItemId;
                    existingEntity.OldItemId = entity.OldItemId;
                    existingEntity.PackTypeId = entity.PackTypeId;
                    existingEntity.OldPackTypeId = entity.OldPackTypeId;
                    existingEntity.StartPackNo = entity.StartPackNo;
                    existingEntity.EndPackNo = entity.EndPackNo;
                    existingEntity.NetWeightPerPack = entity.NetWeightPerPack;
                    existingEntity.TotalBags = entity.TotalBags;
                    existingEntity.NetWeight = entity.NetWeight;
                    existingEntity.WarehouseId = entity.WarehouseId;
                    existingEntity.BinId = entity.BinId;
                    existingEntity.LooseConeKgs = entity.LooseConeKgs;
                    existingEntity.LooseHandlingId = entity.LooseHandlingId;
                    existingEntity.FaultId = entity.FaultId;
                    existingEntity.WasteTypeId = entity.WasteTypeId;
                    existingEntity.WasteQuantity = entity.WasteQuantity;
                    existingEntity.WasteReason = entity.WasteReason;
                    existingEntity.Remarks = entity.Remarks;
                    existingEntity.LotId = entity.LotId;
                    existingEntity.IsActive = entity.IsActive;

                    // Replace details: remove old, add new
                    if (existingEntity.RepackingDetails != null)
                    {
                        _applicationDbContext.RepackingDetail.RemoveRange(existingEntity.RepackingDetails);
                    }

                    // Fetch source info per detail (provides OldTotalBags for pack-no calc)
                    var sourceInfoMapUpd = new Dictionary<int, StockPackSourceDto?>();
                    if (entity.RepackingDetails != null)
                    {
                        foreach (var detail in entity.RepackingDetails)
                        {
                            sourceInfoMapUpd[detail.OldStartPackNo] = await _salesStockLedgerService.GetPackSourceInfoAsync(
                                detail.OldStartPackNo, detail.OldEndPackNo,
                                existingEntity.ProductionYear, existingEntity.UnitId);
                        }
                    }

                    // Auto-calculate StartPackNo/EndPackNo for each detail
                    if (isRepacking)
                    {
                        int totalNewBagsUpd = existingEntity.EndPackNo - existingEntity.StartPackNo + 1;
                        int totalOldBagsUpd = entity.RepackingDetails?.Sum(d =>
                            sourceInfoMapUpd.GetValueOrDefault(d.OldStartPackNo)?.OldTotalBags ?? 0) ?? 0;

                        if (entity.RepackingDetails != null)
                        {
                            int nextPackNoUpd = existingEntity.StartPackNo;
                            var detailListUpd = entity.RepackingDetails.ToList();
                            for (int idx = 0; idx < detailListUpd.Count; idx++)
                            {
                                var detail = detailListUpd[idx];
                                bool isLast = idx == detailListUpd.Count - 1;
                                detail.StartPackNo = nextPackNoUpd;
                                if (isLast)
                                {
                                    detail.EndPackNo = existingEntity.EndPackNo;
                                }
                                else
                                {
                                    var oldBags = sourceInfoMapUpd.GetValueOrDefault(detail.OldStartPackNo)?.OldTotalBags ?? 0;
                                    int newBags = totalOldBagsUpd > 0
                                        ? (int)Math.Round((double)oldBags / totalOldBagsUpd * totalNewBagsUpd)
                                        : 0;
                                    detail.EndPackNo = nextPackNoUpd + newBags - 1;
                                    nextPackNoUpd = detail.EndPackNo + 1;
                                }
                            }
                        }
                    }
                    else
                    {
                        // YarnConversion: keep same pack numbers — source packs deleted, new items inserted at same PackNos
                        if (entity.RepackingDetails != null)
                        {
                            foreach (var detail in entity.RepackingDetails)
                            {
                                detail.StartPackNo = detail.OldStartPackNo;
                                detail.EndPackNo = detail.OldEndPackNo;
                            }
                        }
                    }

                    if (entity.RepackingDetails != null)
                    {
                        foreach (var detail in entity.RepackingDetails)
                        {
                            detail.RepackHeaderId = existingEntity.Id;
                            _applicationDbContext.RepackingDetail.Add(detail);
                        }
                    }

                    _applicationDbContext.RepackingHeader.Update(existingEntity);
                    await _applicationDbContext.SaveChangesAsync();

                    // Inherit LotId from source packs (for repacking)
                    var lotId = existingEntity.LotId;
                    if (isRepacking && entity.RepackingDetails?.Count > 0)
                    {
                        var firstDetail = entity.RepackingDetails.First();
                        lotId = await _salesStockLedgerService.GetLotIdByPackRangeAsync(
                            firstDetail.OldStartPackNo, firstDetail.OldEndPackNo,
                            existingEntity.ProductionYear, existingEntity.UnitId);
                    }

                    // Insert new target packs per detail row (DetailDocNo = RepackingDetail.Id)
                    var stockEntries = new List<SalesStockLedgerDto>();
                    var savedDetails = await _applicationDbContext.RepackingDetail
                        .Where(d => d.RepackHeaderId == existingEntity.Id)
                        .ToListAsync();
                    foreach (var detail in savedDetails)
                    {
                        int currentPackNo = detail.StartPackNo;
                        int packCount = detail.EndPackNo - detail.StartPackNo + 1;
                        for (int i = 0; i < packCount; i++)
                        {
                            stockEntries.Add(new SalesStockLedgerDto
                            {
                                UnitId = existingEntity.UnitId,
                                DocType = "PROD",
                                DocNo = existingEntity.Id,
                                DetailDocNo = detail.Id,
                                DocDate = existingEntity.RepackDate.ToDateTime(TimeOnly.MinValue),
                                ItemId = existingEntity.ItemId,
                                LotId = lotId,
                                PackNo = currentPackNo++,
                                PackTypeId = existingEntity.PackTypeId,
                                WarehouseId = existingEntity.WarehouseId,
                                BinId = existingEntity.BinId,
                                TotalQty = 1,
                                TotalValue = existingEntity.NetWeightPerPack,
                                StatusId = packedStatusId,
                                TypeId = salesStockTypeId
                            });
                        }
                    }

                    await _salesStockLedgerService.InsertAsync(stockEntries);

                    // Upsert ProductionStockLedger — one row per (UnitId, ItemId, LotId, DocDate)
                    bool keyChanged = oldOldItemId != existingEntity.OldItemId || oldRepackDate != existingEntity.RepackDate;

                    if (keyChanged)
                    {
                        var oldLedger = await _applicationDbContext.ProductionStockLedger
                            .FirstOrDefaultAsync(l => l.UnitId == existingEntity.UnitId
                                && l.ItemId == oldOldItemId && l.DocDate == oldRepackDate);
                        if (oldLedger != null)
                        {
                            if (oldLedger.ProdKgs > 0 || oldLedger.TotalBags > 0 || oldLedger.NetWeight > 0)
                            {
                                // Has PACK data — clear REPACK fields and restore ClosingLooseKgs to PACK-only
                                oldLedger.BagsRepacked     = 0;
                                oldLedger.RepackKgs        = 0;
                                oldLedger.ClosingLooseKgs -= oldLooseConeKgs;
                            }
                            else
                                _applicationDbContext.ProductionStockLedger.Remove(oldLedger);
                        }
                    }

                    var repackLedger = await _applicationDbContext.ProductionStockLedger
                        .FirstOrDefaultAsync(l => l.UnitId == existingEntity.UnitId
                            && l.ItemId == existingEntity.OldItemId && l.LotId == lotId
                            && l.DocDate == existingEntity.RepackDate);

                    if (repackLedger != null)
                    {
                        repackLedger.BagsRepacked = existingEntity.TotalBags;
                        repackLedger.RepackKgs    = existingEntity.NetWeight;

                        if (keyChanged)
                        {
                            // Different row — add REPACK loose on top of any existing PACK loose
                            if (repackLedger.ProdKgs > 0 || repackLedger.TotalBags > 0 || repackLedger.NetWeight > 0)
                                repackLedger.ClosingLooseKgs += existingEntity.LooseConeKgs;
                            else
                                repackLedger.ClosingLooseKgs = existingEntity.LooseConeKgs;
                        }
                        else
                        {
                            // Same row — replace old REPACK loose portion with new
                            if (repackLedger.ProdKgs > 0 || repackLedger.TotalBags > 0 || repackLedger.NetWeight > 0)
                                repackLedger.ClosingLooseKgs = repackLedger.ClosingLooseKgs - oldLooseConeKgs + existingEntity.LooseConeKgs;
                            else
                                repackLedger.ClosingLooseKgs = existingEntity.LooseConeKgs;
                        }
                    }
                    else
                    {
                        await _applicationDbContext.ProductionStockLedger.AddAsync(new ProductionStockLedger
                        {
                            UnitId           = existingEntity.UnitId,
                            ItemId           = existingEntity.OldItemId,
                            LotId            = lotId,
                            DocDate          = existingEntity.RepackDate,
                            OpeningLooseKgs  = 0,
                            ProdKgs          = 0,
                            TotalProdKgs     = 0,
                            PackTypeId       = existingEntity.PackTypeId,
                            NetWeightPerPack = existingEntity.NetWeightPerPack,
                            TotalBags        = 0,
                            NetWeight        = 0,
                            BagsRepacked     = existingEntity.TotalBags,
                            RepackKgs        = existingEntity.NetWeight,
                            ClosingLooseKgs  = existingEntity.LooseConeKgs,
                            ClosingPackKgs   = 0,
                            ClosingBags      = 0
                        });
                    }

                    // Mark previous date entries as stock-closed
                    var prevEntries = await _applicationDbContext.ProductionStockLedger
                        .Where(l => l.UnitId == existingEntity.UnitId && l.ItemId == existingEntity.OldItemId
                            && l.LotId == lotId && l.DocDate < existingEntity.RepackDate && !l.StockClosing)
                        .ToListAsync();
                    foreach (var prev in prevEntries)
                        prev.StockClosing = true;

                    await _applicationDbContext.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return existingEntity.Id;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.RepackingHeader
                .Include(h => h.RepackingDetails)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync(ct);
                try
                {
                    var packedStatus = await _salesMiscMasterLookup.GetByCodeAsync(MiscEnumEntity.Packed);
                    var packedStatusId = packedStatus?.Id ?? 0;

                    var deletedStatus = await _salesMiscMasterLookup.GetByCodeAsync(MiscEnumEntity.Deleted);
                    var deletedStatusId = deletedStatus?.Id ?? 0;

                    // Restore source packs back to Packed for each detail
                    if (existing.RepackingDetails != null)
                    {
                        foreach (var detail in existing.RepackingDetails)
                        {
                            await _salesStockLedgerService.UpdateStatusByPackRangeAsync(
                                "PROD",
                                detail.OldStartPackNo, detail.OldEndPackNo,
                                deletedStatusId, packedStatusId,
                                existing.ProductionYear, existing.UnitId, ct);
                        }
                    }

                    // Delete StockLedger entries for this doc
                    await _salesStockLedgerService.DeleteByDocAsync(
                        "PROD", existing.Id, existing.ProductionYear, existing.UnitId, ct);

                    // Clean up ProductionStockLedger — one row per (UnitId, ItemId, LotId, DocDate)
                    bool isRepacking = existing.ItemId == existing.OldItemId;
                    var lotId = existing.LotId;
                    if (isRepacking && existing.RepackingDetails?.Count > 0)
                    {
                        var firstDetail = existing.RepackingDetails.First();
                        lotId = await _salesStockLedgerService.GetLotIdByPackRangeAsync(
                            firstDetail.OldStartPackNo, firstDetail.OldEndPackNo,
                            existing.ProductionYear, existing.UnitId);
                    }

                    var ledgerEntry = await _applicationDbContext.ProductionStockLedger
                        .FirstOrDefaultAsync(l => l.UnitId == existing.UnitId
                            && l.ItemId == existing.OldItemId && l.LotId == lotId
                            && l.DocDate == existing.RepackDate, ct);

                    if (ledgerEntry != null)
                    {
                        if (ledgerEntry.ProdKgs > 0 || ledgerEntry.TotalBags > 0 || ledgerEntry.NetWeight > 0)
                        {
                            // Has PACK data — clear REPACK fields and restore ClosingLooseKgs to PACK-only value
                            ledgerEntry.BagsRepacked = 0;
                            ledgerEntry.RepackKgs = 0;
                            var packDetail = await _applicationDbContext.ProductionPackEntryDetail
                                .Where(d => d.ProductionPackEntry.UnitId == existing.UnitId
                                    && d.ProductionPackEntry.ItemId == existing.OldItemId
                                    && d.LotId == lotId
                                    && d.ProductionPackEntry.PackDate == existing.RepackDate
                                    && d.ProductionPackEntry.IsDeleted == IsDelete.NotDeleted)
                                .FirstOrDefaultAsync(ct);
                            ledgerEntry.ClosingLooseKgs = packDetail?.LooseConeKgs ?? 0;
                        }
                        else
                        {
                            _applicationDbContext.ProductionStockLedger.Remove(ledgerEntry);
                        }
                    }

                    // Soft delete the header (details cascade)
                    existing.IsDeleted = IsDelete.Deleted;
                    _applicationDbContext.RepackingHeader.Update(existing);
                    await _applicationDbContext.SaveChangesAsync(ct);

                    await transaction.CommitAsync(ct);
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync(ct);
                    throw;
                }
            });
        }
    }
}
