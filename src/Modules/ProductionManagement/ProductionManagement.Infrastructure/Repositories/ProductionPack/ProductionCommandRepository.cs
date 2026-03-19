using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Application.Common.Interfaces.IMiscMaster;
using ProductionManagement.Application.Common.Interfaces.IProductionPack;
using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;
using ProductionManagement.Infrastructure.Data;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Infrastructure.Repositories.ProductionPack
{
    public class ProductionCommandRepository : IProductionCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IDbConnection _dbConnection;

        public ProductionCommandRepository(
            ApplicationDbContext applicationDbContext,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IDbConnection dbConnection)
        {
            _applicationDbContext = applicationDbContext;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _dbConnection = dbConnection;
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

                        var packedStatus = await _miscMasterQueryRepository.GetMiscMasterByCode(
                             MiscEnumEntity.Packed);
                        var packedStatusId = packedStatus?.Id ?? 0;

                        // Insert StockLedger rows via Dapper (cross-module write to Sales.StockLedger)
                        const string insertSql = @"
                            INSERT INTO Sales.StockLedger
                                (UnitId, DocType, DocNo, DetailDocNo, DocDate, ItemId, LotId, PackNo, PackTypeId, WarehouseId, BinId, TotalQty, TotalValue, StatusId)
                            VALUES
                                (@UnitId, @DocType, @DocNo, @DetailDocNo, @DocDate, @ItemId, @LotId, @PackNo, @PackTypeId, @WarehouseId, @BinId, @TotalQty, @TotalValue, @StatusId)";

                        foreach (var detail in details)
                        {
                            for (int packNo = detail.StartPackNo; packNo <= detail.EndPackNo; packNo++)
                            {
                                await _dbConnection.ExecuteAsync(insertSql, new
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
                    }

                    await _applicationDbContext.Database.ExecuteSqlRawAsync(
                        "UPDATE [Finance].[DocumentSequence] SET DocNo = DocNo + 1 WHERE TransactionTypeId = {0} AND IsDeleted = 0",
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

                    // Delete old StockLedger entries via Dapper (cross-module write to Sales.StockLedger)
                    if (existingEntity.ProductionPackDetails != null && existingEntity.ProductionPackDetails.Any())
                    {
                        await _dbConnection.ExecuteAsync(
                            "DELETE FROM Sales.StockLedger WHERE DocType = 'PROD' AND DocNo = @DocNo",
                            new { DocNo = existingEntity.Id });

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

                        var packedStatus = await _miscMasterQueryRepository.GetMiscMasterByCode(
                                                MiscEnumEntity.Packed);
                        var packedStatusId = packedStatus?.Id ?? 0;

                        // Insert new StockLedger rows via Dapper
                        const string insertSql = @"
                            INSERT INTO Sales.StockLedger
                                (UnitId, DocType, DocNo, DetailDocNo, DocDate, ItemId, LotId, PackNo, PackTypeId, WarehouseId, BinId, TotalQty, TotalValue, StatusId)
                            VALUES
                                (@UnitId, @DocType, @DocNo, @DetailDocNo, @DocDate, @ItemId, @LotId, @PackNo, @PackTypeId, @WarehouseId, @BinId, @TotalQty, @TotalValue, @StatusId)";

                        foreach (var detail in newDetails)
                        {
                            for (int packNo = detail.StartPackNo; packNo <= detail.EndPackNo; packNo++)
                            {
                                await _dbConnection.ExecuteAsync(insertSql, new
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
