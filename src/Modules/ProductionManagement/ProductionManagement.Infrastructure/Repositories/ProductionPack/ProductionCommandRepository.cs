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
                                DetailDocNo = 0,
                                DocDate     = entity.PackDate.ToDateTime(TimeOnly.MinValue),
                                ItemId      = entity.ItemId,
                                LotId       = entity.LotId,
                                PackNo      = packNo,
                                PackTypeId  = entity.PackTypeId ?? 0,
                                WarehouseId = entity.WarehouseId,
                                BinId       = entity.BinId ?? 0,
                                TotalQty    = 1,
                                TotalValue  = entity.NetWeightPerPack ?? 0,
                                StatusId    = packedStatusId
                            });
                        }

                        await _salesStockLedgerLookup.InsertAsync(stockEntries);
                    }

                    // Upsert ProductionStockLedger — one row per (UnitId, ItemId, LotId, DocDate)
                    // Get previous date's cumulative closing for running balance
                    var prevPackLedger = await _applicationDbContext.ProductionStockLedger
                        .Where(l => l.UnitId == entity.UnitId && l.ItemId == entity.ItemId
                            && l.LotId == entity.LotId && l.DocDate < entity.PackDate)
                        .OrderByDescending(l => l.DocDate)
                        .ThenByDescending(l => l.Id)
                        .FirstOrDefaultAsync();
                    var prevClosingPackKgs = prevPackLedger?.ClosingPackKgs ?? 0;
                    var prevClosingBags    = prevPackLedger?.ClosingBags ?? 0;

                    var existingLedger = await _applicationDbContext.ProductionStockLedger
                        .FirstOrDefaultAsync(l => l.UnitId == entity.UnitId
                            && l.ItemId == entity.ItemId && l.LotId == entity.LotId
                            && l.DocDate == entity.PackDate);

                    if (existingLedger != null)
                    {
                        existingLedger.OpeningLooseKgs  = entity.OpeningLooseKgs;
                        existingLedger.ProdKgs          = entity.ProductionKgs;
                        existingLedger.TotalProdKgs     = entity.TotalProductionKgs;
                        existingLedger.PackTypeId       = entity.PackTypeId ?? 0;
                        existingLedger.NetWeightPerPack = entity.NetWeightPerPack ?? 0;
                        existingLedger.TotalBags        = entity.TotalBags;
                        existingLedger.NetWeight        = entity.TotalNetWeight;
                        existingLedger.ClosingLooseKgs  = entity.LooseConeKgs;
                        existingLedger.ClosingPackKgs   = prevClosingPackKgs + entity.TotalNetWeight;
                        existingLedger.ClosingBags      = prevClosingBags + entity.TotalBags;
                    }
                    else
                    {
                        await _applicationDbContext.ProductionStockLedger.AddAsync(new ProductionStockLedger
                        {
                            UnitId           = entity.UnitId,
                            ItemId           = entity.ItemId,
                            LotId            = entity.LotId,
                            DocDate          = entity.PackDate,
                            OpeningLooseKgs  = entity.OpeningLooseKgs,
                            ProdKgs          = entity.ProductionKgs,
                            TotalProdKgs     = entity.TotalProductionKgs,
                            PackTypeId       = entity.PackTypeId ?? 0,
                            NetWeightPerPack = entity.NetWeightPerPack ?? 0,
                            TotalBags        = entity.TotalBags,
                            NetWeight        = entity.TotalNetWeight,
                            BagsRepacked     = 0,
                            RepackKgs        = 0,
                            ClosingLooseKgs  = entity.LooseConeKgs,
                            ClosingPackKgs   = prevClosingPackKgs + entity.TotalNetWeight,
                            ClosingBags      = prevClosingBags + entity.TotalBags
                        });
                    }

                    // Mark previous date entries as stock-closed
                    var prevEntries = await _applicationDbContext.ProductionStockLedger
                        .Where(l => l.UnitId == entity.UnitId && l.ItemId == entity.ItemId
                            && l.LotId == entity.LotId && l.DocDate < entity.PackDate && !l.StockClosing)
                        .ToListAsync();
                    foreach (var prev in prevEntries)
                        prev.StockClosing = true;

                    await _applicationDbContext.SaveChangesAsync();

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
                    // Save old key for ledger cleanup
                    var oldUnitId = existingEntity.UnitId;
                    var oldItemId = existingEntity.ItemId;
                    var oldLotId = existingEntity.LotId;
                    var oldPackDate = existingEntity.PackDate;

                    existingEntity.PackDate          = entity.PackDate;
                    existingEntity.ProductionYear    = entity.ProductionYear;
                    existingEntity.UnitId            = entity.UnitId;
                    existingEntity.WarehouseId       = entity.WarehouseId;
                    existingEntity.ItemId            = entity.ItemId;
                    existingEntity.LotId             = entity.LotId;
                    existingEntity.PackTypeId        = entity.PackTypeId;
                    existingEntity.NetWeightPerPack  = entity.NetWeightPerPack;
                    existingEntity.StartPackNo       = entity.StartPackNo;
                    existingEntity.EndPackNo         = entity.EndPackNo;
                    existingEntity.OpeningLooseKgs   = entity.OpeningLooseKgs;
                    existingEntity.TotalProductionKgs = entity.TotalProductionKgs;
                    existingEntity.TotalBags         = entity.TotalBags;
                    existingEntity.TotalNetWeight    = entity.TotalNetWeight;
                    existingEntity.ProductionKgs     = entity.ProductionKgs;
                    existingEntity.LooseConeKgs      = entity.LooseConeKgs;
                    existingEntity.BinId            = entity.BinId;
                    existingEntity.QualityStatusId  = entity.QualityStatusId;
                    existingEntity.Remarks          = entity.Remarks;
                    existingEntity.IsActive         = entity.IsActive;

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
                                PackTypeId  = existingEntity.PackTypeId ?? 0,
                                WarehouseId = existingEntity.WarehouseId,
                                BinId       = existingEntity.BinId ?? 0,
                                TotalQty    = 1,
                                TotalValue  = existingEntity.NetWeightPerPack ?? 0,
                                StatusId    = packedStatusId
                            });
                        }

                        await _salesStockLedgerLookup.InsertAsync(stockEntries);
                    }

                    _applicationDbContext.ProductionPackDetail.Update(existingEntity);
                    await _applicationDbContext.SaveChangesAsync();

                    // Upsert ProductionStockLedger — one row per (UnitId, ItemId, LotId, DocDate)
                    // If key changed, clean up old row first
                    if (oldUnitId != existingEntity.UnitId || oldItemId != existingEntity.ItemId
                        || oldLotId != existingEntity.LotId || oldPackDate != existingEntity.PackDate)
                    {
                        var oldLedger = await _applicationDbContext.ProductionStockLedger
                            .FirstOrDefaultAsync(l => l.UnitId == oldUnitId && l.ItemId == oldItemId
                                && l.LotId == oldLotId && l.DocDate == oldPackDate);
                        if (oldLedger != null)
                        {
                            if (oldLedger.BagsRepacked > 0 || oldLedger.RepackKgs > 0)
                            {
                                // Has REPACK data — only clear PACK fields
                                oldLedger.OpeningLooseKgs = 0;
                                oldLedger.ProdKgs = 0;
                                oldLedger.TotalProdKgs = 0;
                                oldLedger.PackTypeId = 0;
                                oldLedger.NetWeightPerPack = 0;
                                oldLedger.TotalBags = 0;
                                oldLedger.NetWeight = 0;
                                oldLedger.ClosingPackKgs = 0;
                                oldLedger.ClosingBags = 0;
                            }
                            else
                                _applicationDbContext.ProductionStockLedger.Remove(oldLedger);
                        }
                    }

                    // Get previous date's cumulative closing for running balance
                    var prevUpdLedger = await _applicationDbContext.ProductionStockLedger
                        .Where(l => l.UnitId == existingEntity.UnitId && l.ItemId == existingEntity.ItemId
                            && l.LotId == existingEntity.LotId && l.DocDate < existingEntity.PackDate)
                        .OrderByDescending(l => l.DocDate)
                        .ThenByDescending(l => l.Id)
                        .FirstOrDefaultAsync();
                    var prevUpdClosingPackKgs = prevUpdLedger?.ClosingPackKgs ?? 0;
                    var prevUpdClosingBags    = prevUpdLedger?.ClosingBags ?? 0;

                    var ledger = await _applicationDbContext.ProductionStockLedger
                        .FirstOrDefaultAsync(l => l.UnitId == existingEntity.UnitId
                            && l.ItemId == existingEntity.ItemId && l.LotId == existingEntity.LotId
                            && l.DocDate == existingEntity.PackDate);

                    if (ledger != null)
                    {
                        ledger.OpeningLooseKgs  = existingEntity.OpeningLooseKgs;
                        ledger.ProdKgs          = existingEntity.ProductionKgs;
                        ledger.TotalProdKgs     = existingEntity.TotalProductionKgs;
                        ledger.PackTypeId       = existingEntity.PackTypeId ?? 0;
                        ledger.NetWeightPerPack = existingEntity.NetWeightPerPack ?? 0;
                        ledger.TotalBags        = existingEntity.TotalBags;
                        ledger.NetWeight        = existingEntity.TotalNetWeight;
                        ledger.ClosingLooseKgs  = existingEntity.LooseConeKgs;
                        ledger.ClosingPackKgs   = prevUpdClosingPackKgs + existingEntity.TotalNetWeight;
                        ledger.ClosingBags      = prevUpdClosingBags + existingEntity.TotalBags;
                    }
                    else
                    {
                        await _applicationDbContext.ProductionStockLedger.AddAsync(new ProductionStockLedger
                        {
                            UnitId           = existingEntity.UnitId,
                            ItemId           = existingEntity.ItemId,
                            LotId            = existingEntity.LotId,
                            DocDate          = existingEntity.PackDate,
                            OpeningLooseKgs  = existingEntity.OpeningLooseKgs,
                            ProdKgs          = existingEntity.ProductionKgs,
                            TotalProdKgs     = existingEntity.TotalProductionKgs,
                            PackTypeId       = existingEntity.PackTypeId ?? 0,
                            NetWeightPerPack = existingEntity.NetWeightPerPack ?? 0,
                            TotalBags        = existingEntity.TotalBags,
                            NetWeight        = existingEntity.TotalNetWeight,
                            BagsRepacked     = 0,
                            RepackKgs        = 0,
                            ClosingLooseKgs  = existingEntity.LooseConeKgs,
                            ClosingPackKgs   = prevUpdClosingPackKgs + existingEntity.TotalNetWeight,
                            ClosingBags      = prevUpdClosingBags + existingEntity.TotalBags
                        });
                    }

                    // Mark previous date entries as stock-closed
                    var prevEntries = await _applicationDbContext.ProductionStockLedger
                        .Where(l => l.UnitId == existingEntity.UnitId && l.ItemId == existingEntity.ItemId
                            && l.LotId == existingEntity.LotId && l.DocDate < existingEntity.PackDate && !l.StockClosing)
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
    }
}
