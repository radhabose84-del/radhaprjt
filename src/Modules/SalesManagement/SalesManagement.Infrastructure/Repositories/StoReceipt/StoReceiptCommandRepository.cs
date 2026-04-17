using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SalesManagement.Application.Common.Interfaces.IStoReceipt;
using SalesManagement.Domain.Entities;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.StoReceipt
{
    public class StoReceiptCommandRepository : IStoReceiptCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;

        public StoReceiptCommandRepository(
            ApplicationDbContext dbContext,
            IDocumentSequenceLookup documentSequenceLookup)
        {
            _dbContext = dbContext;
            _documentSequenceLookup = documentSequenceLookup;
        }

        public async Task<int> CreateAsync(StoReceiptHeader entity, int packedStatusId, int damagedStatusId, int dispatchedStatusId, int typeId)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            var newId = 0;

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    // Separate details from header
                    var details = entity.StoReceiptDetails?.ToList();
                    entity.StoReceiptDetails = null;

                    // Save header first to get auto-generated Id
                    await _dbContext.StoReceiptHeader.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();


                    // Fetch SourceUnitId (dispatch plant) from Delivery Challan
                    var dcHeader = await _dbContext.DeliveryChallanHeader
                        .AsNoTracking()
                        .Where(dc => dc.Id == entity.DeliveryChallanHeaderId)
                        .Select(dc => new { dc.FromPlantId })
                        .FirstOrDefaultAsync();
                    var sourceUnitId = dcHeader?.FromPlantId;

                    // Insert details and create StockLedger at receiving plant
                    if (details != null && details.Count > 0)
                    {
                        foreach (var detail in details)
                        {
                            var newDetail = new StoReceiptDetail
                            {
                                StoReceiptHeaderId = entity.Id,
                                DeliveryChallanDetailId = detail.DeliveryChallanDetailId,
                                ItemId = detail.ItemId,
                                LotId = detail.LotId,
                                StartPackNo = detail.StartPackNo,
                                EndPackNo = detail.EndPackNo,
                                DispatchQuantity = detail.DispatchQuantity,
                                ReceivedQuantity = detail.ReceivedQuantity,
                                DamageQuantity = detail.DamageQuantity,
                                AcceptedQuantity = detail.AcceptedQuantity,
                                UOMId = detail.UOMId,
                                BagCount = detail.BagCount,
                                NetWeight = detail.NetWeight,
                                GrossWeight = detail.GrossWeight,
                                LineStatusId = detail.LineStatusId
                            };
                            await _dbContext.StoReceiptDetail.AddAsync(newDetail);
                            // Save detail now to get its auto-generated Id (needed for DetailDocNo in StockLedger)
                            await _dbContext.SaveChangesAsync();

                            // Mark ALL dispatched packs at FROM plant as Dispatched (single range UPDATE)
                            if (dispatchedStatusId > 0)
                            {
                                await _dbContext.Database.ExecuteSqlRawAsync(
                                    "UPDATE Sales.StockLedger SET StatusId = {0} WHERE DocType = 'PROD' AND ItemId = {1} AND LotId = {2} AND PackNo >= {3} AND PackNo <= {4}",
                                    dispatchedStatusId, detail.ItemId, detail.LotId, detail.StartPackNo, detail.EndPackNo);
                            }

                            // Fetch PackTypeId and TotalValue once for this lot (same across packs in a PROD batch)
                            var sourceStockInfo = await _dbContext.StockLedger
                                .AsNoTracking()
                                .Where(s => s.DocType == "PROD"
                                    && s.ItemId == detail.ItemId
                                    && s.LotId == detail.LotId
                                    && s.PackNo == detail.StartPackNo)
                                .Select(s => new { s.PackTypeId, s.TotalValue })
                                .FirstOrDefaultAsync();

                            var packTypeId = sourceStockInfo?.PackTypeId ?? 0;
                            var totalValue = sourceStockInfo?.TotalValue ?? 0;

                            // INSERT new StockLedger rows at ReceivingPlant for each PackNo
                            // First AcceptedQuantity packs → Packed status, remaining → Damaged status
                            var acceptedPackCount = (int)detail.AcceptedQuantity;
                            var packIndex = 0;

                            for (int packNo = detail.StartPackNo; packNo <= detail.EndPackNo; packNo++)
                            {
                                var stockStatusId = packIndex < acceptedPackCount
                                    ? packedStatusId
                                    : damagedStatusId;

                                var newStock = new StockLedger
                                {
                                    UnitId = entity.ReceivingPlantId,
                                    DocType = "STOR",
                                    DocNo = entity.Id,
                                    DetailDocNo = newDetail.Id,
                                    DocDate = entity.StoReceiptDate,
                                    ItemId = detail.ItemId,
                                    LotId = detail.LotId,
                                    PackNo = packNo,
                                    PackTypeId = packTypeId,
                                    WarehouseId = entity.ReceivingStorageLocationId,
                                    BinId = entity.BinId ?? 0,
                                    TotalQty = 1,
                                    TotalValue = totalValue,
                                    StatusId = stockStatusId,
                                    SourceUnitId = sourceUnitId
                                };
                                await _dbContext.StockLedger.AddAsync(newStock);
                                packIndex++;
                            }

                            // Save StockLedger rows for this detail
                            await _dbContext.SaveChangesAsync();
                        }
                    }

                    // Increment DocNo via Finance lookup (within same transaction)
                    var dbConnection = _dbContext.Database.GetDbConnection();
                    var dbTransaction = transaction.GetDbTransaction();
                    await _documentSequenceLookup.IncrementDocNoAsync(typeId, dbConnection, dbTransaction);

                    await transaction.CommitAsync();
                    newId = entity.Id;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            return newId;
        }
    }
}
