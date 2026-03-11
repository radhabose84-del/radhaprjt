using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.DeliveryChallan
{
    public class DeliveryChallanCommandRepository : IDeliveryChallanCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public DeliveryChallanCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GenerateNextDeliveryNumberAsync(int fromPlantId, CancellationToken ct = default)
        {
            var prefix = $"DC-{fromPlantId}-";

            var lastNumber = await _dbContext.DeliveryChallanHeader
                .Where(x => x.DeliveryNumber != null && x.DeliveryNumber.StartsWith(prefix))
                .OrderByDescending(x => x.DeliveryNumber)
                .Select(x => x.DeliveryNumber)
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

        public async Task<int> CreateAsync(Domain.Entities.DeliveryChallanHeader entity, int fromPlantId, int packedStatusId, int dispatchedStatusId)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            var newId = 0;

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    // Separate details from header
                    var details = entity.DeliveryChallanDetails?.ToList();
                    entity.DeliveryChallanDetails = null;

                    // Save header first to get auto-generated Id
                    await _dbContext.DeliveryChallanHeader.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();

                    // Insert details and update StockLedger per detail line
                    if (details != null && details.Count > 0)
                    {
                        foreach (var detail in details)
                        {
                            var newDetail = new Domain.Entities.DeliveryChallanDetail
                            {
                                DeliveryChallanHeaderId = entity.Id,
                                StoDetailId = detail.StoDetailId,
                                ItemId = detail.ItemId,
                                LotId = detail.LotId,
                                StartPackNo = detail.StartPackNo,
                                EndPackNo = detail.EndPackNo,
                                DispatchQuantity = detail.DispatchQuantity,
                                UOMId = detail.UOMId,
                                BagCount = detail.BagCount,
                                BaleCount = detail.BaleCount,
                                NetWeight = detail.NetWeight,
                                GrossWeight = detail.GrossWeight,
                                ExMillRate = detail.ExMillRate,
                                LineMovementValue = detail.LineMovementValue
                            };
                            await _dbContext.DeliveryChallanDetail.AddAsync(newDetail);

                            // Update StockLedger: change each PackNo from Packed to Reserved
                            for (int packNo = detail.StartPackNo; packNo <= detail.EndPackNo; packNo++)
                            {
                                var stockRecord = await _dbContext.StockLedger
                                    .FirstOrDefaultAsync(s => s.UnitId == fromPlantId
                                        && s.ItemId == detail.ItemId
                                        && s.LotId == detail.LotId
                                        && s.PackNo == packNo
                                        && s.StatusId == packedStatusId);

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

        public async Task<bool> SoftDeleteAsync(int id, int dispatchedStatusId, int packedStatusId, CancellationToken ct)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            var result = false;

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
                try
                {
                    var existing = await _dbContext.DeliveryChallanHeader
                        .Include(h => h.DeliveryChallanDetails)
                        .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

                    if (existing == null)
                    {
                        result = false;
                        return;
                    }

                    // Reverse StockLedger: change each PackNo from Reserved back to Packed
                    if (existing.DeliveryChallanDetails != null && existing.DeliveryChallanDetails.Count > 0)
                    {
                        foreach (var detail in existing.DeliveryChallanDetails)
                        {
                            for (int packNo = detail.StartPackNo; packNo <= detail.EndPackNo; packNo++)
                            {
                                var stockRecord = await _dbContext.StockLedger
                                    .FirstOrDefaultAsync(s => s.UnitId == existing.FromPlantId
                                        && s.ItemId == detail.ItemId
                                        && s.LotId == detail.LotId
                                        && s.PackNo == packNo
                                        && s.StatusId == dispatchedStatusId, ct);

                                if (stockRecord != null)
                                {
                                    stockRecord.StatusId = packedStatusId;
                                }
                            }
                        }
                    }

                    // Soft delete header
                    existing.IsDeleted = IsDelete.Deleted;
                    _dbContext.DeliveryChallanHeader.Update(existing);
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
