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

        public async Task<int> CreateAsync(StoReceiptHeader entity, int receivingPlantId, int reservedStatusId, int dispatchedStatusId)
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

                    // Insert details and update StockLedger per detail line
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

                            // Update StockLedger: change each PackNo from Reserved to Dispatched
                            for (int packNo = detail.StartPackNo; packNo <= detail.EndPackNo; packNo++)
                            {
                                var stockRecord = await _dbContext.StockLedger
                                    .FirstOrDefaultAsync(s => s.UnitId == receivingPlantId
                                        && s.ItemId == detail.ItemId
                                        && s.LotId == detail.LotId
                                        && s.PackNo == packNo
                                        && s.StatusId == reservedStatusId);

                                if (stockRecord != null)
                                {
                                    stockRecord.StatusId = dispatchedStatusId;
                                }
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

        public async Task<bool> SoftDeleteAsync(int id, int dispatchedStatusId, int reservedStatusId, CancellationToken ct)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            var result = false;

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
                try
                {
                    var existing = await _dbContext.StoReceiptHeader
                        .Include(h => h.StoReceiptDetails)
                        .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

                    if (existing == null)
                    {
                        result = false;
                        return;
                    }

                    // Reverse StockLedger: change each PackNo from Dispatched back to Reserved
                    if (existing.StoReceiptDetails != null && existing.StoReceiptDetails.Count > 0)
                    {
                        var receivingPlantId = existing.ReceivingPlantId;

                        foreach (var detail in existing.StoReceiptDetails)
                        {
                            for (int packNo = detail.StartPackNo; packNo <= detail.EndPackNo; packNo++)
                            {
                                var stockRecord = await _dbContext.StockLedger
                                    .FirstOrDefaultAsync(s => s.UnitId == receivingPlantId
                                        && s.ItemId == detail.ItemId
                                        && s.LotId == detail.LotId
                                        && s.PackNo == packNo
                                        && s.StatusId == dispatchedStatusId, ct);

                                if (stockRecord != null)
                                {
                                    stockRecord.StatusId = reservedStatusId;
                                }
                            }
                        }
                    }

                    // Soft delete header
                    existing.IsDeleted = IsDelete.Deleted;
                    _dbContext.StoReceiptHeader.Update(existing);
                    await _dbContext.SaveChangesAsync(ct);

                    await transaction.CommitAsync(ct);
                    result = true;
                }
                catch
                {
                    await transaction.RollbackAsync(ct);
                    throw;
                }
            });

            return result;
        }
    }
}
