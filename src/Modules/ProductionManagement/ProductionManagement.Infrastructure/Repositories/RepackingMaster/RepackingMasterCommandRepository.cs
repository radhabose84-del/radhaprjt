using Contracts.Dtos.Stock;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProductionManagement.Application.Common.Interfaces.IRepackingMaster;
using ProductionManagement.Domain.Common;
using ProductionManagement.Infrastructure.Data;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Infrastructure.Repositories.RepackingMaster
{
    public class RepackingMasterCommandRepository : IRepackingMasterCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly ISalesMiscMasterLookup _salesMiscMasterLookup;
        private readonly ISalesStockLedgerService _salesStockLedgerLookup;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;

        public RepackingMasterCommandRepository(
            ApplicationDbContext applicationDbContext,
            ISalesMiscMasterLookup salesMiscMasterLookup,
            ISalesStockLedgerService salesStockLedgerLookup,
            IDocumentSequenceLookup documentSequenceLookup)
        {
            _applicationDbContext = applicationDbContext;
            _salesMiscMasterLookup = salesMiscMasterLookup;
            _salesStockLedgerLookup = salesStockLedgerLookup;
            _documentSequenceLookup = documentSequenceLookup;
        }

        public async Task<int> CreateAsync(Domain.Entities.RepackingMaster entity, int typeId)
        {
            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    // Get Packed and Deleted status IDs
                    var packedStatus = await _salesMiscMasterLookup.GetByCodeAsync(MiscEnumEntity.Packed);
                    var packedStatusId = packedStatus?.Id ?? 0;

                    var deletedStatus = await _salesMiscMasterLookup.GetByCodeAsync(MiscEnumEntity.Deleted);
                    var deletedStatusId = deletedStatus?.Id ?? 0;

                    // Get Repacking TypeId from MiscMaster
                    var repackingType = await _salesMiscMasterLookup.GetByCodeAsync("Repacking");
                    var repackingTypeId = repackingType?.Id;

                    // Get last pack number for continuous numbering
                    var lastPackNo = await _salesStockLedgerLookup.GetLastPackNoByYearAsync(entity.ProductionYear, entity.UnitId);
                    entity.StartPackNo = lastPackNo + 1;
                    entity.EndPackNo = entity.StartPackNo + entity.TotalBags - 1;

                    // Save entity
                    await _applicationDbContext.RepackingMaster.AddAsync(entity);
                    await _applicationDbContext.SaveChangesAsync();

                    // Mark source packs as Deleted in StockLedger
                    await _salesStockLedgerLookup.UpdateStatusByPackRangeAsync(
                        "PROD",
                        entity.OldStartPackNo, entity.OldEndPackNo,
                        packedStatusId, deletedStatusId,
                        entity.ProductionYear, entity.UnitId);

                    // Inherit LotId from source packs
                    var lotId = await _salesStockLedgerLookup.GetLotIdByPackRangeAsync(entity.OldStartPackNo, entity.OldEndPackNo, entity.ProductionYear, entity.UnitId);

                    // Insert new packs into StockLedger
                    var stockEntries = new List<SalesStockLedgerDto>();
                    for (int packNo = entity.StartPackNo; packNo <= entity.EndPackNo; packNo++)
                    {
                        stockEntries.Add(new SalesStockLedgerDto
                        {
                            UnitId = entity.UnitId,
                            DocType = "PROD",
                            DocNo = entity.Id,
                            DetailDocNo = 0,
                            DocDate = entity.RepackDate.ToDateTime(TimeOnly.MinValue),
                            ItemId = entity.ItemId,
                            LotId = lotId,
                            PackNo = packNo,
                            PackTypeId = entity.PackTypeId,
                            WarehouseId = entity.WarehouseId,
                            BinId = entity.BinId,
                            TotalQty = 1,
                            TotalValue = entity.NetWeightPerPack,
                            StatusId = packedStatusId,
                            TypeId = repackingTypeId
                        });
                    }

                    await _salesStockLedgerLookup.InsertAsync(stockEntries);

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

        public async Task<int> UpdateAsync(Domain.Entities.RepackingMaster entity)
        {
            var existingEntity = await _applicationDbContext.RepackingMaster
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

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

                    var repackingType = await _salesMiscMasterLookup.GetByCodeAsync("Repacking");
                    var repackingTypeId = repackingType?.Id;

                    // Restore old source packs back to Packed
                    await _salesStockLedgerLookup.UpdateStatusByPackRangeAsync(
                        "PROD",
                        existingEntity.OldStartPackNo, existingEntity.OldEndPackNo,
                        deletedStatusId, packedStatusId,
                        existingEntity.ProductionYear, existingEntity.UnitId);

                    // Delete old RPK StockLedger entries
                    await _salesStockLedgerLookup.DeleteByDocAsync("PROD", existingEntity.Id, existingEntity.ProductionYear, existingEntity.UnitId);

                    // Mark new source packs as Deleted
                    await _salesStockLedgerLookup.UpdateStatusByPackRangeAsync(
                        "PROD",
                        entity.OldStartPackNo, entity.OldEndPackNo,
                        packedStatusId, deletedStatusId,
                        entity.ProductionYear, entity.UnitId);

                    // Recalculate target pack numbers
                    var lastPackNo = await _salesStockLedgerLookup.GetLastPackNoByYearAsync(entity.ProductionYear, entity.UnitId);
                    entity.StartPackNo = lastPackNo + 1;
                    entity.EndPackNo = entity.StartPackNo + entity.TotalBags - 1;

                    // Update entity fields
                    existingEntity.RepackDate = entity.RepackDate;
                    existingEntity.ItemId = entity.ItemId;                    
                    existingEntity.OldPackTypeId = entity.OldPackTypeId;
                    existingEntity.OldNetWeightPerPack = entity.OldNetWeightPerPack;
                    existingEntity.OldStartPackNo = entity.OldStartPackNo;
                    existingEntity.OldEndPackNo = entity.OldEndPackNo;
                    existingEntity.OldTotalBags = entity.OldTotalBags;
                    existingEntity.OldNetWeight = entity.OldNetWeight;
                    existingEntity.OldWarehouseId = entity.OldWarehouseId;
                    existingEntity.OldBinId = entity.OldBinId;
                    existingEntity.PackTypeId = entity.PackTypeId;
                    existingEntity.NetWeightPerPack = entity.NetWeightPerPack;
                    existingEntity.StartPackNo = entity.StartPackNo;
                    existingEntity.EndPackNo = entity.EndPackNo;
                    existingEntity.TotalBags = entity.TotalBags;
                    existingEntity.NetWeight = entity.NetWeight;
                    existingEntity.WarehouseId = entity.WarehouseId;
                    existingEntity.BinId = entity.BinId;
                    existingEntity.LooseConeKgs = entity.LooseConeKgs;
                    existingEntity.LooseHandlingId = entity.LooseHandlingId;
                    existingEntity.Remarks = entity.Remarks;
                    existingEntity.IsActive = entity.IsActive;

                    _applicationDbContext.RepackingMaster.Update(existingEntity);
                    await _applicationDbContext.SaveChangesAsync();

                    // Inherit LotId from new source packs
                    var lotId = await _salesStockLedgerLookup.GetLotIdByPackRangeAsync(existingEntity.OldStartPackNo, existingEntity.OldEndPackNo, existingEntity.ProductionYear, existingEntity.UnitId);

                    // Insert new StockLedger entries
                    var stockEntries = new List<SalesStockLedgerDto>();
                    for (int packNo = existingEntity.StartPackNo; packNo <= existingEntity.EndPackNo; packNo++)
                    {
                        stockEntries.Add(new SalesStockLedgerDto
                        {
                            UnitId = existingEntity.UnitId,
                            DocType = "PROD",
                            DocNo = existingEntity.Id,
                            DetailDocNo = 0,
                            DocDate = existingEntity.RepackDate.ToDateTime(TimeOnly.MinValue),
                            ItemId = existingEntity.ItemId,
                            LotId = lotId,
                            PackNo = packNo,
                            PackTypeId = existingEntity.PackTypeId,
                            WarehouseId = existingEntity.WarehouseId,
                            BinId = existingEntity.BinId,
                            TotalQty = 1,
                            TotalValue = existingEntity.NetWeightPerPack,
                            StatusId = packedStatusId,
                            TypeId = repackingTypeId
                        });
                    }

                    await _salesStockLedgerLookup.InsertAsync(stockEntries);

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
            var existing = await _applicationDbContext.RepackingMaster
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

                    // Restore source packs back to Packed
                    await _salesStockLedgerLookup.UpdateStatusByPackRangeAsync(
                        "PROD", existing.OldStartPackNo, existing.OldEndPackNo,
                        deletedStatusId, packedStatusId,
                        existing.ProductionYear, existing.UnitId,
                        ct);

                    // Delete RPK StockLedger entries
                    await _salesStockLedgerLookup.DeleteByDocAsync("PROD", existing.Id, existing.ProductionYear, existing.UnitId, ct);

                    // Soft delete the entity
                    existing.IsDeleted = IsDelete.Deleted;
                    _applicationDbContext.RepackingMaster.Update(existing);
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
