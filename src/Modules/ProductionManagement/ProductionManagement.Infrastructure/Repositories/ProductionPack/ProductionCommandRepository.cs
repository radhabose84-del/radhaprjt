using Contracts.Dtos.Stock;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProductionManagement.Application.Common.Interfaces.IProductionPack;
using ProductionManagement.Domain.Entities;
using ProductionManagement.Infrastructure.Data;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Infrastructure.Repositories.ProductionPack
{
    public class ProductionCommandRepository : IProductionCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly ISalesMiscMasterLookup _salesMiscMasterLookup;
        private readonly ISalesStockLedgerService _salesStockLedgerLookup;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;

        public ProductionCommandRepository(
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

        public async Task<int> CreateAsync(ProductionPackDetail entity, int typeId)
        {
            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    await _applicationDbContext.ProductionPackDetail.AddAsync(entity);
                    await _applicationDbContext.SaveChangesAsync();

                    // Insert one StockLedger row per physical bag (only when pack range exists)
                    if (entity.StartPackNo.HasValue && entity.EndPackNo.HasValue)
                    {
                        var packedStatus = await _salesMiscMasterLookup.GetByCodeAsync("Packed");
                        var packedStatusId = packedStatus?.Id ?? 0;

                        var stockEntries = new List<SalesStockLedgerDto>();
                        for (int packNo = entity.StartPackNo.Value; packNo <= entity.EndPackNo.Value; packNo++)
                        {
                            stockEntries.Add(new SalesStockLedgerDto
                            {
                                UnitId      = entity.UnitId,
                                DocType     = "PROD",
                                DocNo       = entity.Id,
                                DetailDocNo = entity.Id,
                                DocDate     = entity.PackDate.ToDateTime(TimeOnly.MinValue),
                                ItemId      = entity.ItemId,
                                LotId       = entity.LotId,
                                PackNo      = packNo,
                                PackTypeId  = entity.PackTypeId,
                                WarehouseId = entity.WarehouseId,
                                BinId       = entity.BinId ?? 0,
                                TotalQty    = 1,
                                TotalValue  = entity.NetWeightPerPack,
                                StatusId    = packedStatusId
                            });
                        }

                        await _salesStockLedgerLookup.InsertAsync(stockEntries);
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

        public async Task<int> UpdateAsync(ProductionPackDetail entity)
        {
            var existingEntity = await _applicationDbContext.ProductionPackDetail
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    existingEntity.PackDate       = entity.PackDate;
                    existingEntity.ProductionYear = entity.ProductionYear;
                    existingEntity.UnitId         = entity.UnitId;
                    existingEntity.WarehouseId    = entity.WarehouseId;
                    existingEntity.ItemId         = entity.ItemId;
                    existingEntity.LotId          = entity.LotId;
                    existingEntity.PackTypeId     = entity.PackTypeId;
                    existingEntity.NetWeightPerPack = entity.NetWeightPerPack;
                    existingEntity.StartPackNo    = entity.StartPackNo;
                    existingEntity.EndPackNo      = entity.EndPackNo;
                    existingEntity.NoOfBags       = entity.NoOfBags;
                    existingEntity.TotalBags      = entity.TotalBags;
                    existingEntity.TotalNetWeight = entity.TotalNetWeight;
                    existingEntity.ProductionKgs  = entity.ProductionKgs;
                    existingEntity.LooseConeKgs   = entity.LooseConeKgs;
                    existingEntity.BinId          = entity.BinId;
                    existingEntity.QualityStatusId = entity.QualityStatusId;
                    existingEntity.Remarks        = entity.Remarks;
                    existingEntity.IsActive       = entity.IsActive;

                    // Delete existing stock ledger rows for this doc, then re-insert
                    await _salesStockLedgerLookup.DeleteByDocAsync(
                        "PROD", existingEntity.Id, existingEntity.ProductionYear, existingEntity.UnitId);

                    if (existingEntity.StartPackNo.HasValue && existingEntity.EndPackNo.HasValue)
                    {
                        var packedStatus = await _salesMiscMasterLookup.GetByCodeAsync("Packed");
                        var packedStatusId = packedStatus?.Id ?? 0;

                        var stockEntries = new List<SalesStockLedgerDto>();
                        for (int packNo = existingEntity.StartPackNo.Value; packNo <= existingEntity.EndPackNo.Value; packNo++)
                        {
                            stockEntries.Add(new SalesStockLedgerDto
                            {
                                UnitId      = existingEntity.UnitId,
                                DocType     = "PROD",
                                DocNo       = existingEntity.Id,
                                DetailDocNo = existingEntity.Id,
                                DocDate     = existingEntity.PackDate.ToDateTime(TimeOnly.MinValue),
                                ItemId      = existingEntity.ItemId,
                                LotId       = existingEntity.LotId,
                                PackNo      = packNo,
                                PackTypeId  = existingEntity.PackTypeId,
                                WarehouseId = existingEntity.WarehouseId,
                                BinId       = existingEntity.BinId ?? 0,
                                TotalQty    = 1,
                                TotalValue  = existingEntity.NetWeightPerPack,
                                StatusId    = packedStatusId
                            });
                        }

                        await _salesStockLedgerLookup.InsertAsync(stockEntries);
                    }

                    _applicationDbContext.ProductionPackDetail.Update(existingEntity);
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
    }
}
