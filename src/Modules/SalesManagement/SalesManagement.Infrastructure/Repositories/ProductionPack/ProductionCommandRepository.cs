using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.IProductionPack;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.ProductionPack
{
    public class ProductionCommandRepository : IProductionCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;

        public ProductionCommandRepository(
            ApplicationDbContext applicationDbContext,
            IMiscMasterQueryRepository miscMasterQueryRepository)
        {
            _applicationDbContext = applicationDbContext;
            _miscMasterQueryRepository = miscMasterQueryRepository;
        }

        public async Task<int> CreateAsync(ProductionPackHeader entity, int typeId)
        {
            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    // Step 1: Separate details from header
                    var details = entity.ProductionPackDetails?.ToList();
                    entity.ProductionPackDetails = null;

                    // Step 2: Save header → get HeaderId
                    await _applicationDbContext.ProductionPackHeader.AddAsync(entity);
                    await _applicationDbContext.SaveChangesAsync();

                    // Step 3: Save details → get each Detail.Id, then generate StockLedger rows
                    if (details != null && details.Count > 0)
                    {
                        foreach (var detail in details)
                        {
                            detail.ProductionPackHeaderId = entity.Id;
                        }
                        await _applicationDbContext.ProductionPackDetail.AddRangeAsync(details);
                        await _applicationDbContext.SaveChangesAsync();

                        // Step 4: Fetch "Packed" status from MiscMaster
                        var packedStatus = await _miscMasterQueryRepository.GetMiscMasterByCode(
                             MiscEnumEntity.Packed);
                        var packedStatusId = packedStatus?.Id ?? 0;

                        // Step 5: Generate StockLedger rows from each detail's pack range
                        var stockLedgerEntries = new List<StockLedger>();
                        foreach (var detail in details)
                        {
                            for (int packNo = detail.StartPackNo; packNo <= detail.EndPackNo; packNo++)
                            {
                                stockLedgerEntries.Add(new StockLedger
                                {
                                    UnitId = entity.UnitId,
                                    DocType = "PROD",
                                    DocNo = entity.Id,
                                    DetailDocNo = detail.Id,
                                    DocDate = entity.PackDate,
                                    ItemId = detail.ItemId,
                                    LotId = detail.LotId,
                                    PackNo = packNo,
                                    PackTypeId = detail.PackTypeId,
                                    WarehouseId = entity.WarehouseId,
                                    BinId = detail.BinId,
                                    TotalQty = 1,
                                    TotalValue = detail.NetWeightPerPack,
                                    StatusId = packedStatusId
                                });
                            }
                        }

                        if (stockLedgerEntries.Count > 0)
                        {
                            await _applicationDbContext.StockLedger.AddRangeAsync(stockLedgerEntries);
                            await _applicationDbContext.SaveChangesAsync();
                        }
                    }

                    // Step 6: Increment DocNo in DocumentSequence
                    await _applicationDbContext.Database.ExecuteSqlRawAsync(
                        "UPDATE [Finance].[DocumentSequence] SET DocNo = DocNo + 1 WHERE TypeId = {0} AND IsDeleted = 0",
                        typeId);

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

        public async Task<int> UpdateAsync(ProductionPackHeader entity)
        {
            var existingEntity = await _applicationDbContext.ProductionPackHeader
                .Include(h => h.ProductionPackDetails)
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    // Update header fields (StatusId removed from header)
                    existingEntity.PackDate = entity.PackDate;
                    existingEntity.UnitId = entity.UnitId;
                    existingEntity.WarehouseId = entity.WarehouseId;
                    existingEntity.TotalBags = entity.TotalBags;
                    existingEntity.TotalNetWeight = entity.TotalNetWeight;
                    existingEntity.ProductionKgs = entity.ProductionKgs;
                    existingEntity.LooseKgs = entity.LooseKgs;
                    existingEntity.Remarks = entity.Remarks;
                    existingEntity.IsActive = entity.IsActive;

                    // Remove old StockLedger entries for existing details
                    if (existingEntity.ProductionPackDetails != null && existingEntity.ProductionPackDetails.Any())
                    {
                        var oldStockLedger = await _applicationDbContext.StockLedger
                            .Where(sl => sl.DocType == "PROD" && sl.DocNo == existingEntity.Id)
                            .ToListAsync();

                        if (oldStockLedger.Count > 0)
                        {
                            _applicationDbContext.StockLedger.RemoveRange(oldStockLedger);
                        }

                        // Remove existing details
                        _applicationDbContext.ProductionPackDetail.RemoveRange(existingEntity.ProductionPackDetails);
                    }

                    // Insert new details + StockLedger
                    if (entity.ProductionPackDetails != null && entity.ProductionPackDetails.Any())
                    {
                        var newDetails = entity.ProductionPackDetails.ToList();
                        foreach (var detail in newDetails)
                        {
                            detail.ProductionPackHeaderId = existingEntity.Id;
                        }
                        await _applicationDbContext.ProductionPackDetail.AddRangeAsync(newDetails);
                        await _applicationDbContext.SaveChangesAsync();

                        // Fetch "Packed" status from MiscMaster
                        var packedStatus = await _miscMasterQueryRepository.GetMiscMasterByCode(
                                                MiscEnumEntity.Packed);
                        var packedStatusId = packedStatus?.Id ?? 0;

                        // Generate new StockLedger rows
                        var stockLedgerEntries = new List<StockLedger>();
                        foreach (var detail in newDetails)
                        {
                            for (int packNo = detail.StartPackNo; packNo <= detail.EndPackNo; packNo++)
                            {
                                stockLedgerEntries.Add(new StockLedger
                                {
                                    UnitId = existingEntity.UnitId,
                                    DocType = "PROD",
                                    DocNo = existingEntity.Id,
                                    DetailDocNo = detail.Id,
                                    DocDate = existingEntity.PackDate,
                                    ItemId = detail.ItemId,
                                    LotId = detail.LotId,
                                    PackNo = packNo,
                                    PackTypeId = detail.PackTypeId,
                                    WarehouseId = existingEntity.WarehouseId,
                                    BinId = detail.BinId,
                                    TotalQty = 1,
                                    TotalValue = detail.NetWeightPerPack,
                                    StatusId = packedStatusId
                                });
                            }
                        }

                        if (stockLedgerEntries.Count > 0)
                        {
                            await _applicationDbContext.StockLedger.AddRangeAsync(stockLedgerEntries);
                        }
                    }

                    _applicationDbContext.ProductionPackHeader.Update(existingEntity);
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
