using Contracts.Dtos.Stock;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProductionManagement.Application.Common.Interfaces.IRepackingHeader;
using ProductionManagement.Domain.Common;
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
                    var stockType = await _salesMiscMasterLookup.GetByCodeAsync(typeCode);
                    var stockTypeId = stockType?.Id;

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

                    // Auto-calculate StartPackNo/EndPackNo for each detail from header.StartPackNo
                    int nextPackNo = entity.StartPackNo;
                    if (entity.RepackingDetails != null)
                    {
                        foreach (var detail in entity.RepackingDetails)
                        {
                            var oldTotalBags = sourceInfoMap.GetValueOrDefault(detail.OldStartPackNo)?.OldTotalBags ?? 0;
                            detail.StartPackNo = nextPackNo;
                            detail.EndPackNo = nextPackNo + oldTotalBags - 1;
                            nextPackNo = detail.EndPackNo + 1;
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

                    // Inherit LotId from source packs (for repacking; yarn conversion uses header LotId)
                    var lotId = entity.LotId ?? 0;
                    if (isRepacking && entity.RepackingDetails?.Count > 0)
                    {
                        var firstDetail = entity.RepackingDetails.First();
                        lotId = await _salesStockLedgerService.GetLotIdByPackRangeAsync(
                            firstDetail.OldStartPackNo, firstDetail.OldEndPackNo,
                            entity.ProductionYear, entity.UnitId);
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
                                    TypeId = stockTypeId
                                });
                            }
                        }
                    }

                    await _salesStockLedgerService.InsertAsync(stockEntries);

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
                    var stockType = await _salesMiscMasterLookup.GetByCodeAsync(typeCode);
                    var stockTypeId = stockType?.Id;

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

                    // Auto-calculate StartPackNo/EndPackNo for each detail from header.StartPackNo
                    int nextPackNoUpd = existingEntity.StartPackNo;
                    if (entity.RepackingDetails != null)
                    {
                        foreach (var detail in entity.RepackingDetails)
                        {
                            var oldTotalBags = sourceInfoMapUpd.GetValueOrDefault(detail.OldStartPackNo)?.OldTotalBags ?? 0;
                            detail.StartPackNo = nextPackNoUpd;
                            detail.EndPackNo = nextPackNoUpd + oldTotalBags - 1;
                            nextPackNoUpd = detail.EndPackNo + 1;
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
                    var lotId = existingEntity.LotId ?? 0;
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
                                TypeId = stockTypeId
                            });
                        }
                    }

                    await _salesStockLedgerService.InsertAsync(stockEntries);

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
