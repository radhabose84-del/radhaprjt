using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IStoReceipt;
using SalesManagement.Domain.Entities;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.StoReceipt
{
    public class StoReceiptCommandRepository : IStoReceiptCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public StoReceiptCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GenerateNextStoReceiptNumberAsync(int receivingPlantId, CancellationToken ct = default)
        {
            var prefix = $"STOR-{receivingPlantId}-";

            var lastNumber = await _dbContext.StoReceiptHeader
                .Where(x => x.StoReceiptNumber != null && x.StoReceiptNumber.StartsWith(prefix))
                .OrderByDescending(x => x.StoReceiptNumber)
                .Select(x => x.StoReceiptNumber)
                .FirstOrDefaultAsync(ct);

            var nextSeq = 1;
            if (lastNumber != null)
            {
                var seqPart = lastNumber.Substring(prefix.Length);
                if (int.TryParse(seqPart, out var lastSeq))
                {
                    nextSeq = lastSeq + 1;
                }
            }

            return $"{prefix}{nextSeq:D5}";
        }

        public async Task<int> CreateAsync(StoReceiptHeader entity, int packedStatusId)
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

                    // Get FromPlantId from the linked DeliveryChallan
                    var dcHeader = await _dbContext.DeliveryChallanHeader
                        .Where(dc => dc.Id == entity.DeliveryChallanHeaderId && dc.IsDeleted == IsDelete.NotDeleted)
                        .Select(dc => new { dc.FromPlantId, dc.FromStorageLocationId })
                        .FirstOrDefaultAsync();

                    var fromPlantId = dcHeader?.FromPlantId ?? 0;

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

                            // INSERT new StockLedger rows at ReceivingPlant for each PackNo
                            for (int packNo = detail.StartPackNo; packNo <= detail.EndPackNo; packNo++)
                            {
                                // Fetch PackTypeId and TotalValue from the FromPlant's StockLedger
                                var sourceStock = await _dbContext.StockLedger
                                    .FirstOrDefaultAsync(s => s.UnitId == fromPlantId
                                        && s.ItemId == detail.ItemId
                                        && s.LotId == detail.LotId
                                        && s.PackNo == packNo);

                                var packTypeId = sourceStock?.PackTypeId ?? 0;
                                var totalValue = sourceStock?.TotalValue ?? 0;

                                // Insert new StockLedger at receiving plant with Packed status
                                var newStock = new StockLedger
                                {
                                    UnitId = entity.ReceivingPlantId,
                                    DocType = "STOR",
                                    DocNo = entity.Id,
                                    DetailDocNo = newDetail.Id > 0 ? newDetail.Id : 0,
                                    DocDate = entity.StoReceiptDate,
                                    ItemId = detail.ItemId,
                                    LotId = detail.LotId,
                                    PackNo = packNo,
                                    PackTypeId = packTypeId,
                                    WarehouseId = entity.ReceivingStorageLocationId,
                                    BinId = entity.BinId ?? 0,
                                    TotalQty = 1,
                                    TotalValue = totalValue,
                                    StatusId = packedStatusId
                                };
                                await _dbContext.StockLedger.AddAsync(newStock);
                            }
                        }

                        await _dbContext.SaveChangesAsync();
                    }

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
