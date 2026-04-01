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

        public async Task<int> CreateAsync(ProductionPackHeader entity, int typeId)
        {
            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    var details = entity.ProductionPackDetails?.ToList();
                    entity.ProductionPackDetails = null;

                    await _applicationDbContext.ProductionPackHeader.AddAsync(entity);
                    await _applicationDbContext.SaveChangesAsync();

                    if (details != null && details.Count > 0)
                    {
                        foreach (var detail in details)
                        {
                            detail.ProductionPackHeaderId = entity.Id;
                        }
                        await _applicationDbContext.ProductionPackDetail.AddRangeAsync(details);
                        await _applicationDbContext.SaveChangesAsync();

                        var packedStatus = await _salesMiscMasterLookup.GetByCodeAsync("Packed");
                        var packedStatusId = packedStatus?.Id ?? 0;

                        var stockEntries = new List<SalesStockLedgerDto>();
                        foreach (var detail in details)
                        {
                            for (int packNo = detail.StartPackNo; packNo <= detail.EndPackNo; packNo++)
                            {
                                stockEntries.Add(new SalesStockLedgerDto
                                {
                                    UnitId = entity.UnitId,
                                    DocType = "PROD",
                                    DocNo = entity.Id,
                                    DetailDocNo = detail.Id,
                                    DocDate = entity.PackDate.ToDateTime(TimeOnly.MinValue),
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

                        await _salesStockLedgerLookup.InsertAsync(stockEntries);
                    }

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
                    existingEntity.PackDate = entity.PackDate;
                    existingEntity.ProductionYear = entity.ProductionYear;
                    existingEntity.UnitId = entity.UnitId;
                    existingEntity.WarehouseId = entity.WarehouseId;
                    existingEntity.TotalBags = entity.TotalBags;
                    existingEntity.TotalNetWeight = entity.TotalNetWeight;
                    existingEntity.ProductionKgs = entity.ProductionKgs;
                    existingEntity.LooseConeKgs = entity.LooseConeKgs;
                    existingEntity.Remarks = entity.Remarks;
                    existingEntity.IsActive = entity.IsActive;

                    if (existingEntity.ProductionPackDetails != null && existingEntity.ProductionPackDetails.Any())
                    {
                        await _salesStockLedgerLookup.DeleteByDocAsync("PROD", existingEntity.Id, existingEntity.ProductionYear, existingEntity.UnitId);

                        _applicationDbContext.ProductionPackDetail.RemoveRange(existingEntity.ProductionPackDetails);
                    }

                    if (entity.ProductionPackDetails != null && entity.ProductionPackDetails.Any())
                    {
                        var newDetails = entity.ProductionPackDetails.ToList();
                        foreach (var detail in newDetails)
                        {
                            detail.ProductionPackHeaderId = existingEntity.Id;
                        }
                        await _applicationDbContext.ProductionPackDetail.AddRangeAsync(newDetails);
                        await _applicationDbContext.SaveChangesAsync();

                        var packedStatus = await _salesMiscMasterLookup.GetByCodeAsync("Packed");
                        var packedStatusId = packedStatus?.Id ?? 0;

                        var stockEntries = new List<SalesStockLedgerDto>();
                        foreach (var detail in newDetails)
                        {
                            for (int packNo = detail.StartPackNo; packNo <= detail.EndPackNo; packNo++)
                            {
                                stockEntries.Add(new SalesStockLedgerDto
                                {
                                    UnitId = existingEntity.UnitId,
                                    DocType = "PROD",
                                    DocNo = existingEntity.Id,
                                    DetailDocNo = detail.Id,
                                    DocDate = existingEntity.PackDate.ToDateTime(TimeOnly.MinValue),
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

                        await _salesStockLedgerLookup.InsertAsync(stockEntries);
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
