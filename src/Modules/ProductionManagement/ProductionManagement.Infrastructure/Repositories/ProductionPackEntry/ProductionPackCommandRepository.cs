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

        public async Task<int> CreateAsync(ProductionPackEntry entity, int typeId)
        {
            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    // EF Core cascades details on AddAsync — header + details saved together
                    await _applicationDbContext.ProductionPackEntry.AddAsync(entity);
                    await _applicationDbContext.SaveChangesAsync();

                    // Insert SalesStockLedger rows per detail (one row per physical bag)
                    var packedStatus = await _salesMiscMasterLookup.GetByCodeAsync("Packed");
                    var packedStatusId = packedStatus?.Id ?? 0;
                    var ledgerItemId = entity.VariantId ?? entity.ItemId;

                    if (entity.Details != null)
                    {
                        var stockEntries = new List<SalesStockLedgerDto>();
                        foreach (var detail in entity.Details)
                        {
                            if (detail.StartPackNo.HasValue && detail.EndPackNo.HasValue)
                            {
                                for (int packNo = detail.StartPackNo.Value; packNo <= detail.EndPackNo.Value; packNo++)
                                {
                                    stockEntries.Add(new SalesStockLedgerDto
                                    {
                                        UnitId      = entity.UnitId,
                                        DocType     = "PROD",
                                        DocNo       = entity.Id,
                                        DetailDocNo = detail.Id,
                                        DocDate     = entity.PackDate.ToDateTime(TimeOnly.MinValue),
                                        ItemId      = ledgerItemId,
                                        LotId       = detail.LotId,
                                        PackNo      = packNo,
                                        PackTypeId  = detail.PackTypeId ?? 0,
                                        WarehouseId = entity.WarehouseId,
                                        BinId       = entity.BinId ?? 0,
                                        TotalQty    = 1,
                                        TotalValue  = detail.NetWeightPerPack ?? 0,
                                        StatusId    = packedStatusId
                                    });
                                }
                            }
                        }

                        if (stockEntries.Count > 0)
                            await _salesStockLedgerLookup.InsertAsync(stockEntries);

                        // Upsert ProductionStockLedger per detail — one row per (UnitId, ItemId, LotId, DocDate)
                        foreach (var detail in entity.Details)
                        {
                            await UpsertProductionStockLedgerAsync(entity, detail, ledgerItemId);
                        }
                    }

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

        public async Task<int> UpdateAsync(ProductionPackEntry entity)
        {
            var existingEntity = await _applicationDbContext.ProductionPackEntry
                .Include(e => e.Details)
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    // Save old detail lot IDs for ledger cleanup
                    var oldLotIds = existingEntity.Details?
                        .Select(d => d.LotId).Distinct().ToList() ?? new List<int>();
                    var oldItemId = existingEntity.VariantId ?? existingEntity.ItemId;
                    var oldUnitId = existingEntity.UnitId;
                    var oldPackDate = existingEntity.PackDate;

                    // Update header fields
                    existingEntity.PackDate          = entity.PackDate;
                    existingEntity.ProductionYear    = entity.ProductionYear;
                    existingEntity.UnitId            = entity.UnitId;
                    existingEntity.WarehouseId       = entity.WarehouseId;
                    existingEntity.ItemId            = entity.ItemId;
                    existingEntity.VariantId         = entity.VariantId;
                    existingEntity.BinId             = entity.BinId;
                    existingEntity.QualityStatusId   = entity.QualityStatusId;
                    existingEntity.IsActive          = entity.IsActive;

                    // Replace details: remove old, add new
                    if (existingEntity.Details != null)
                        _applicationDbContext.ProductionPackEntryDetail.RemoveRange(existingEntity.Details);

                    if (entity.Details != null)
                    {
                        foreach (var detail in entity.Details)
                        {
                            detail.Id = 0; // Reset Id for new insert
                            detail.ProductionPackEntryId = existingEntity.Id;
                        }
                        await _applicationDbContext.ProductionPackEntryDetail.AddRangeAsync(entity.Details);
                    }

                    // Delete existing stock ledger rows for this doc, then re-insert
                    await _salesStockLedgerLookup.DeleteByDocAsync(
                        "PROD", existingEntity.Id, existingEntity.ProductionYear, existingEntity.UnitId);

                    var packedStatus = await _salesMiscMasterLookup.GetByCodeAsync("Packed");
                    var packedStatusId = packedStatus?.Id ?? 0;
                    var updLedgerItemId = existingEntity.VariantId ?? existingEntity.ItemId;

                    _applicationDbContext.ProductionPackEntry.Update(existingEntity);
                    await _applicationDbContext.SaveChangesAsync();

                    // Re-insert SalesStockLedger per new detail
                    if (entity.Details != null)
                    {
                        var stockEntries = new List<SalesStockLedgerDto>();
                        foreach (var detail in entity.Details)
                        {
                            if (detail.StartPackNo.HasValue && detail.EndPackNo.HasValue)
                            {
                                for (int packNo = detail.StartPackNo.Value; packNo <= detail.EndPackNo.Value; packNo++)
                                {
                                    stockEntries.Add(new SalesStockLedgerDto
                                    {
                                        UnitId      = existingEntity.UnitId,
                                        DocType     = "PROD",
                                        DocNo       = existingEntity.Id,
                                        DetailDocNo = detail.Id,
                                        DocDate     = existingEntity.PackDate.ToDateTime(TimeOnly.MinValue),
                                        ItemId      = updLedgerItemId,
                                        LotId       = detail.LotId,
                                        PackNo      = packNo,
                                        PackTypeId  = detail.PackTypeId ?? 0,
                                        WarehouseId = existingEntity.WarehouseId,
                                        BinId       = existingEntity.BinId ?? 0,
                                        TotalQty    = 1,
                                        TotalValue  = detail.NetWeightPerPack ?? 0,
                                        StatusId    = packedStatusId
                                    });
                                }
                            }
                        }

                        if (stockEntries.Count > 0)
                            await _salesStockLedgerLookup.InsertAsync(stockEntries);
                    }

                    // Clean up old ProductionStockLedger rows for lots that are no longer in the new details
                    var newLotIds = entity.Details?
                        .Select(d => d.LotId).Distinct().ToList() ?? new List<int>();

                    foreach (var oldLotId in oldLotIds)
                    {
                        bool keyChanged = oldUnitId != existingEntity.UnitId
                            || oldItemId != updLedgerItemId
                            || oldPackDate != existingEntity.PackDate
                            || !newLotIds.Contains(oldLotId);

                        if (keyChanged)
                        {
                            var oldLedger = await _applicationDbContext.ProductionStockLedger
                                .FirstOrDefaultAsync(l => l.UnitId == oldUnitId && l.ItemId == oldItemId
                                    && l.LotId == oldLotId && l.DocDate == oldPackDate);
                            if (oldLedger != null)
                            {
                                if (oldLedger.BagsRepacked > 0 || oldLedger.RepackKgs > 0)
                                {
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
                    }

                    // Upsert ProductionStockLedger per new detail
                    if (entity.Details != null)
                    {
                        foreach (var detail in entity.Details)
                        {
                            await UpsertProductionStockLedgerAsync(existingEntity, detail, updLedgerItemId);
                        }
                    }

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

        public async Task<int> StockCloseAsync(DateOnly closingDate, int unitId, CancellationToken ct)
        {
            // 1. Close any existing unclosed entries up to the closing date
            var entries = await _applicationDbContext.ProductionStockLedger
                .Where(l => l.UnitId == unitId
                    && l.DocDate <= closingDate
                    && !l.StockClosing)
                .ToListAsync(ct);

            foreach (var entry in entries)
                entry.StockClosing = true;

            // 2. If no entry exists at all for the closing date, create a single
            //    placeholder row with zero data and mark it as closed.
            var hasEntryOnDate = entries.Any(e => e.DocDate == closingDate)
                || await _applicationDbContext.ProductionStockLedger
                    .AnyAsync(l => l.UnitId == unitId && l.DocDate == closingDate, ct);

            int created = 0;
            if (!hasEntryOnDate)
            {
                await _applicationDbContext.ProductionStockLedger.AddAsync(new ProductionStockLedger
                {
                    UnitId           = unitId,
                    ItemId           = 0,
                    LotId            = 0,
                    DocDate          = closingDate,
                    OpeningLooseKgs  = 0,
                    ProdKgs          = 0,
                    TotalProdKgs     = 0,
                    PackTypeId       = 0,
                    NetWeightPerPack = 0,
                    TotalBags        = 0,
                    NetWeight        = 0,
                    BagsRepacked     = 0,
                    RepackKgs        = 0,
                    ClosingLooseKgs  = 0,
                    ClosingPackKgs   = 0,
                    ClosingBags      = 0,
                    StockClosing     = true
                }, ct);
                created = 1;
            }

            await _applicationDbContext.SaveChangesAsync(ct);
            return entries.Count + created;
        }

        private async Task UpsertProductionStockLedgerAsync(
            ProductionPackEntry header,
            ProductionPackEntryDetail detail,
            int ledgerItemId)
        {
            var prevPackLedger = await _applicationDbContext.ProductionStockLedger
                .Where(l => l.UnitId == header.UnitId && l.ItemId == ledgerItemId
                    && l.LotId == detail.LotId && l.DocDate < header.PackDate)
                .OrderByDescending(l => l.DocDate)
                .ThenByDescending(l => l.Id)
                .FirstOrDefaultAsync();
            var prevClosingPackKgs = prevPackLedger?.ClosingPackKgs ?? 0;
            var prevClosingBags    = prevPackLedger?.ClosingBags ?? 0;

            var existingLedger = await _applicationDbContext.ProductionStockLedger
                .FirstOrDefaultAsync(l => l.UnitId == header.UnitId
                    && l.ItemId == ledgerItemId && l.LotId == detail.LotId
                    && l.DocDate == header.PackDate);

            if (existingLedger != null)
            {
                existingLedger.OpeningLooseKgs  = detail.OpeningLooseKgs;
                existingLedger.ProdKgs          = detail.ProductionKgs;
                existingLedger.TotalProdKgs     = detail.TotalProductionKgs;
                existingLedger.PackTypeId       = detail.PackTypeId ?? 0;
                existingLedger.NetWeightPerPack = detail.NetWeightPerPack ?? 0;
                existingLedger.TotalBags        = detail.TotalBags;
                existingLedger.NetWeight        = detail.TotalNetWeight;
                existingLedger.ClosingLooseKgs  = detail.LooseConeKgs;
                existingLedger.ClosingPackKgs   = prevClosingPackKgs + detail.TotalNetWeight;
                existingLedger.ClosingBags      = prevClosingBags + detail.TotalBags;
            }
            else
            {
                await _applicationDbContext.ProductionStockLedger.AddAsync(new ProductionStockLedger
                {
                    UnitId           = header.UnitId,
                    ItemId           = ledgerItemId,
                    LotId            = detail.LotId,
                    DocDate          = header.PackDate,
                    OpeningLooseKgs  = detail.OpeningLooseKgs,
                    ProdKgs          = detail.ProductionKgs,
                    TotalProdKgs     = detail.TotalProductionKgs,
                    PackTypeId       = detail.PackTypeId ?? 0,
                    NetWeightPerPack = detail.NetWeightPerPack ?? 0,
                    TotalBags        = detail.TotalBags,
                    NetWeight        = detail.TotalNetWeight,
                    BagsRepacked     = 0,
                    RepackKgs        = 0,
                    ClosingLooseKgs  = detail.LooseConeKgs,
                    ClosingPackKgs   = prevClosingPackKgs + detail.TotalNetWeight,
                    ClosingBags      = prevClosingBags + detail.TotalBags
                });
            }
        }
    }
}
