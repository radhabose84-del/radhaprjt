using Contracts.Dtos.Stock;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProductionManagement.Application.Common.Interfaces.IYarnConversionHeader;
using ProductionManagement.Domain.Common;
using ProductionManagement.Infrastructure.Data;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Infrastructure.Repositories.YarnConversionHeader
{
    public class YarnConversionHeaderCommandRepository : IYarnConversionHeaderCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly ISalesMiscMasterLookup _salesMiscMasterLookup;
        private readonly ISalesStockLedgerService _salesStockLedgerService;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;

        public YarnConversionHeaderCommandRepository(
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

        public async Task<int> CreateAsync(Domain.Entities.YarnConversionHeader entity, int typeId)
        {
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

                    var yarnConversionType = await _salesMiscMasterLookup.GetByCodeAsync("YarnConversion");
                    var yarnConversionTypeId = yarnConversionType?.Id;

                    // Calculate target pack range
                    var lastPackNo = await _salesStockLedgerService.GetLastPackNoByYearAsync(entity.ProductionYear, entity.UnitId);
                    entity.StartPackNo = lastPackNo + 1;
                    entity.EndPackNo = entity.StartPackNo + entity.TotalBags - 1;

                    // Save entity
                    await _applicationDbContext.YarnConversionHeader.AddAsync(entity);
                    await _applicationDbContext.SaveChangesAsync();

                    // Mark source packs as Deleted in StockLedger
                    await _salesStockLedgerService.UpdateStatusByPackRangeAsync(
                        "PROD",
                        entity.OldStartPackNo, entity.OldEndPackNo,
                        packedStatusId, deletedStatusId,
                        entity.ProductionYear, entity.UnitId);

                    // Insert new target packs into StockLedger
                    var stockEntries = new List<SalesStockLedgerDto>();
                    for (int packNo = entity.StartPackNo; packNo <= entity.EndPackNo; packNo++)
                    {
                        stockEntries.Add(new SalesStockLedgerDto
                        {
                            UnitId = entity.UnitId,
                            DocType = "PROD",
                            DocNo = entity.Id,
                            DetailDocNo = 0,
                            DocDate = entity.ConversionDate.ToDateTime(TimeOnly.MinValue),
                            ItemId = entity.ItemId,
                            LotId = entity.LotId,
                            PackNo = packNo,
                            PackTypeId = entity.PackTypeId,
                            WarehouseId = entity.WarehouseId,
                            BinId = entity.BinId,
                            TotalQty = 1,
                            TotalValue = entity.NetWeightPerPack,
                            StatusId = packedStatusId,
                            TypeId = yarnConversionTypeId
                        });
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

        public async Task<int> UpdateAsync(Domain.Entities.YarnConversionHeader entity)
        {
            var existingEntity = await _applicationDbContext.YarnConversionHeader
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

                    var yarnConversionType = await _salesMiscMasterLookup.GetByCodeAsync("YarnConversion");
                    var yarnConversionTypeId = yarnConversionType?.Id;

                    // Restore old source packs back to Packed
                    await _salesStockLedgerService.UpdateStatusByPackRangeAsync(
                        "PROD",
                        existingEntity.OldStartPackNo, existingEntity.OldEndPackNo,
                        deletedStatusId, packedStatusId,
                        existingEntity.ProductionYear, existingEntity.UnitId);

                    // Delete old StockLedger entries for this doc
                    await _salesStockLedgerService.DeleteByDocAsync("PROD", existingEntity.Id, existingEntity.ProductionYear, existingEntity.UnitId);

                    // Mark new source packs as Deleted
                    await _salesStockLedgerService.UpdateStatusByPackRangeAsync(
                        "PROD",
                        entity.OldStartPackNo, entity.OldEndPackNo,
                        packedStatusId, deletedStatusId,
                        entity.ProductionYear, entity.UnitId);

                    // Recalculate target pack range
                    var lastPackNo = await _salesStockLedgerService.GetLastPackNoByYearAsync(entity.ProductionYear, entity.UnitId);
                    entity.StartPackNo = lastPackNo + 1;
                    entity.EndPackNo = entity.StartPackNo + entity.TotalBags - 1;

                    // Update entity fields
                    existingEntity.ConversionDate   = entity.ConversionDate;
                    existingEntity.LotId            = entity.LotId;
                    existingEntity.OldItemId        = entity.OldItemId;
                    existingEntity.OldPackTypeId    = entity.OldPackTypeId;
                    existingEntity.OldStartPackNo   = entity.OldStartPackNo;
                    existingEntity.OldEndPackNo     = entity.OldEndPackNo;
                    existingEntity.OldTotalBags     = entity.OldTotalBags;
                    existingEntity.OldNetWeightPerPack = entity.OldNetWeightPerPack;
                    existingEntity.OldNetWeight     = entity.OldNetWeight;
                    existingEntity.OldWarehouseId   = entity.OldWarehouseId;
                    existingEntity.OldBinId         = entity.OldBinId;
                    existingEntity.FaultId          = entity.FaultId;
                    existingEntity.ItemId           = entity.ItemId;
                    existingEntity.PackTypeId       = entity.PackTypeId;
                    existingEntity.TotalBags        = entity.TotalBags;
                    existingEntity.NetWeightPerPack = entity.NetWeightPerPack;
                    existingEntity.NetWeight        = entity.NetWeight;
                    existingEntity.StartPackNo      = entity.StartPackNo;
                    existingEntity.EndPackNo        = entity.EndPackNo;
                    existingEntity.LooseQty         = entity.LooseQty;
                    existingEntity.LooseHandlingId  = entity.LooseHandlingId;
                    existingEntity.WarehouseId      = entity.WarehouseId;
                    existingEntity.BinId            = entity.BinId;
                    existingEntity.WasteTypeId      = entity.WasteTypeId;
                    existingEntity.WasteQty         = entity.WasteQty;
                    existingEntity.WasteReason      = entity.WasteReason;
                    existingEntity.Remarks          = entity.Remarks;
                    existingEntity.IsActive         = entity.IsActive;

                    _applicationDbContext.YarnConversionHeader.Update(existingEntity);
                    await _applicationDbContext.SaveChangesAsync();

                    // Insert new StockLedger entries
                    var stockEntries = new List<SalesStockLedgerDto>();
                    for (int packNo = existingEntity.StartPackNo; packNo <= existingEntity.EndPackNo; packNo++)
                    {
                        stockEntries.Add(new SalesStockLedgerDto
                        {
                            UnitId      = existingEntity.UnitId,
                            DocType     = "PROD",
                            DocNo       = existingEntity.Id,
                            DetailDocNo = 0,
                            DocDate     = existingEntity.ConversionDate.ToDateTime(TimeOnly.MinValue),
                            ItemId      = existingEntity.ItemId,
                            LotId       = existingEntity.LotId,
                            PackNo      = packNo,
                            PackTypeId  = existingEntity.PackTypeId,
                            WarehouseId = existingEntity.WarehouseId,
                            BinId       = existingEntity.BinId,
                            TotalQty    = 1,
                            TotalValue  = existingEntity.NetWeightPerPack,
                            StatusId    = packedStatusId,
                            TypeId      = yarnConversionTypeId
                        });
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
            var existing = await _applicationDbContext.YarnConversionHeader
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
                    await _salesStockLedgerService.UpdateStatusByPackRangeAsync(
                        "PROD", existing.OldStartPackNo, existing.OldEndPackNo,
                        deletedStatusId, packedStatusId,
                        existing.ProductionYear, existing.UnitId, ct);

                    // Delete StockLedger entries for this doc
                    await _salesStockLedgerService.DeleteByDocAsync("PROD", existing.Id, existing.ProductionYear, existing.UnitId, ct);

                    // Soft delete the entity
                    existing.IsDeleted = IsDelete.Deleted;
                    _applicationDbContext.YarnConversionHeader.Update(existing);
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
