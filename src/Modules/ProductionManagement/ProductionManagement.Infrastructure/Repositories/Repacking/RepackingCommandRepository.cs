using Contracts.Dtos.Stock;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProductionManagement.Application.Common.Interfaces.IRepacking;
using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;
using ProductionManagement.Infrastructure.Data;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Infrastructure.Repositories.Repacking
{
    public class RepackingCommandRepository : IRepackingCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly ISalesMiscMasterLookup _salesMiscMasterLookup;
        private readonly ISalesStockLedgerLookup _salesStockLedgerLookup;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;

        public RepackingCommandRepository(
            ApplicationDbContext applicationDbContext,
            ISalesMiscMasterLookup salesMiscMasterLookup,
            ISalesStockLedgerLookup salesStockLedgerLookup,
            IDocumentSequenceLookup documentSequenceLookup)
        {
            _applicationDbContext = applicationDbContext;
            _salesMiscMasterLookup = salesMiscMasterLookup;
            _salesStockLedgerLookup = salesStockLedgerLookup;
            _documentSequenceLookup = documentSequenceLookup;
        }

        public async Task<int> CreateAsync(RepackingHeader entity, int typeId)
        {
            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    var details = entity.RepackingDetails?.ToList();
                    entity.RepackingDetails = null;

                    await _applicationDbContext.RepackingHeader.AddAsync(entity);
                    await _applicationDbContext.SaveChangesAsync();

                    if (details != null && details.Count > 0)
                    {
                        foreach (var detail in details)
                            detail.RepackingHeaderId = entity.Id;

                        await _applicationDbContext.RepackingDetail.AddRangeAsync(details);
                        await _applicationDbContext.SaveChangesAsync();

                        // Resolve status IDs from MiscMaster
                        var packedStatus  = await _salesMiscMasterLookup.GetByCodeAsync(MiscEnumEntity.Packed);
                        var deletedStatus = await _salesMiscMasterLookup.GetByCodeAsync(MiscEnumEntity.Deleted);
                        var packedStatusId  = packedStatus?.Id  ?? 0;
                        var deletedStatusId = deletedStatus?.Id ?? 0;

                        var newStockEntries = new List<SalesStockLedgerDto>();

                        foreach (var detail in details)
                        {
                            // Fetch original pack detail to get the full source range
                            var oldPackDetail = await _applicationDbContext.ProductionPackDetail
                                .FirstOrDefaultAsync(x => x.Id == detail.OldPackDetailId);

                            if (oldPackDetail != null)
                            {
                                // Mark ALL source packs (full original range) as Deleted in StockLedger.
                                // This covers both the repacked packs (detail.StartPackNo–EndPackNo)
                                // and the remaining packs (EndPackNo+1–oldPackDetail.EndPackNo).
                                // Only packs currently in 'Packed' status are affected (currentStatusId guard).
                                await _salesStockLedgerLookup.UpdateStatusByPackRangeAsync(
                                    "PROD", entity.OldPackHeaderId,
                                    oldPackDetail.StartPackNo, oldPackDetail.EndPackNo,
                                    packedStatusId, deletedStatusId);
                            }

                            // Insert new RPK stock entries for the repacked range only
                            for (int packNo = detail.StartPackNo; packNo <= detail.EndPackNo; packNo++)
                            {
                                newStockEntries.Add(new SalesStockLedgerDto
                                {
                                    UnitId      = entity.UnitId,
                                    DocType     = "RPAK",
                                    DocNo       = entity.Id,
                                    DetailDocNo = detail.Id,
                                    DocDate     = entity.RepackingDate.ToDateTime(TimeOnly.MinValue),
                                    ItemId      = detail.ItemId,
                                    LotId       = detail.LotId,
                                    PackNo      = packNo,
                                    PackTypeId  = detail.PackTypeId,
                                    WarehouseId = detail.WarehouseId,
                                    BinId       = detail.BinId,
                                    TotalQty    = 1,
                                    TotalValue  = detail.NetWeightPerPack,
                                    StatusId    = packedStatusId
                                });
                            }
                        }

                        if (newStockEntries.Count > 0)
                            await _salesStockLedgerLookup.InsertAsync(newStockEntries);
                    }

                    var dbConnection  = _applicationDbContext.Database.GetDbConnection();
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

        public async Task<int> UpdateAsync(RepackingHeader entity)
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
                    var packedStatus  = await _salesMiscMasterLookup.GetByCodeAsync(MiscEnumEntity.Packed);
                    var deletedStatus = await _salesMiscMasterLookup.GetByCodeAsync(MiscEnumEntity.Deleted);
                    var packedStatusId  = packedStatus?.Id  ?? 0;
                    var deletedStatusId = deletedStatus?.Id ?? 0;

                    // 1. Remove existing RPK stock entries for this repacking document
                    await _salesStockLedgerLookup.DeleteByDocAsync("RPK", existingEntity.Id);

                    // 2. Restore original PROD packs back to Packed status
                    if (existingEntity.RepackingDetails != null && existingEntity.RepackingDetails.Any())
                    {
                        foreach (var oldDetail in existingEntity.RepackingDetails)
                        {
                            var oldPackDetail = await _applicationDbContext.ProductionPackDetail
                                .FirstOrDefaultAsync(x => x.Id == oldDetail.OldPackDetailId);

                            if (oldPackDetail != null)
                            {
                                await _salesStockLedgerLookup.UpdateStatusByPackRangeAsync(
                                    "PROD", existingEntity.OldPackHeaderId,
                                    oldPackDetail.StartPackNo, oldPackDetail.EndPackNo,
                                    deletedStatusId, packedStatusId);
                            }
                        }

                        _applicationDbContext.RepackingDetail.RemoveRange(existingEntity.RepackingDetails);
                    }

                    // 3. Update header fields
                    existingEntity.RepackingDate    = entity.RepackingDate;
                    existingEntity.TotalBags        = entity.TotalBags;
                    existingEntity.NetWeight        = entity.NetWeight;
                    existingEntity.LooseConeKgs     = entity.LooseConeKgs;
                    existingEntity.OldPackHeaderId  = entity.OldPackHeaderId;
                    existingEntity.LooseHandlingId  = entity.LooseHandlingId;
                    existingEntity.Remarks          = entity.Remarks;
                    existingEntity.IsActive         = entity.IsActive;

                    // 4. Save new details and recreate stock ledger entries
                    if (entity.RepackingDetails != null && entity.RepackingDetails.Any())
                    {
                        var newDetails = entity.RepackingDetails.ToList();
                        foreach (var detail in newDetails)
                            detail.RepackingHeaderId = existingEntity.Id;

                        await _applicationDbContext.RepackingDetail.AddRangeAsync(newDetails);
                        await _applicationDbContext.SaveChangesAsync();

                        var newStockEntries = new List<SalesStockLedgerDto>();

                        foreach (var detail in newDetails)
                        {
                            var oldPackDetail = await _applicationDbContext.ProductionPackDetail
                                .FirstOrDefaultAsync(x => x.Id == detail.OldPackDetailId);

                            if (oldPackDetail != null)
                            {
                                // Re-mark new source packs as Deleted
                                await _salesStockLedgerLookup.UpdateStatusByPackRangeAsync(
                                    "PROD", existingEntity.OldPackHeaderId,
                                    oldPackDetail.StartPackNo, oldPackDetail.EndPackNo,
                                    packedStatusId, deletedStatusId);
                            }

                            for (int packNo = detail.StartPackNo; packNo <= detail.EndPackNo; packNo++)
                            {
                                newStockEntries.Add(new SalesStockLedgerDto
                                {
                                    UnitId      = existingEntity.UnitId,
                                    DocType     = "RPK",
                                    DocNo       = existingEntity.Id,
                                    DetailDocNo = detail.Id,
                                    DocDate     = existingEntity.RepackingDate.ToDateTime(TimeOnly.MinValue),
                                    ItemId      = detail.ItemId,
                                    LotId       = detail.LotId,
                                    PackNo      = packNo,
                                    PackTypeId  = detail.PackTypeId,
                                    WarehouseId = detail.WarehouseId,
                                    BinId       = detail.BinId,
                                    TotalQty    = 1,
                                    TotalValue  = detail.NetWeightPerPack,
                                    StatusId    = packedStatusId
                                });
                            }
                        }

                        if (newStockEntries.Count > 0)
                            await _salesStockLedgerLookup.InsertAsync(newStockEntries);
                    }

                    _applicationDbContext.RepackingHeader.Update(existingEntity);
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
                    var packedStatus  = await _salesMiscMasterLookup.GetByCodeAsync(MiscEnumEntity.Packed);
                    var deletedStatus = await _salesMiscMasterLookup.GetByCodeAsync(MiscEnumEntity.Deleted);
                    var packedStatusId  = packedStatus?.Id  ?? 0;
                    var deletedStatusId = deletedStatus?.Id ?? 0;

                    // Restore PROD packs back to Packed before removing this repacking
                    if (existing.RepackingDetails != null && existing.RepackingDetails.Any())
                    {
                        foreach (var detail in existing.RepackingDetails)
                        {
                            var oldPackDetail = await _applicationDbContext.ProductionPackDetail
                                .FirstOrDefaultAsync(x => x.Id == detail.OldPackDetailId, ct);

                            if (oldPackDetail != null)
                            {
                                await _salesStockLedgerLookup.UpdateStatusByPackRangeAsync(
                                    "PROD", existing.OldPackHeaderId,
                                    oldPackDetail.StartPackNo, oldPackDetail.EndPackNo,
                                    deletedStatusId, packedStatusId);
                            }
                        }
                    }

                    // Remove RPK stock entries
                    await _salesStockLedgerLookup.DeleteByDocAsync("RPK", existing.Id);

                    // Soft delete the header
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
