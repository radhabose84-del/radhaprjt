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

        public async Task<int> CreateAsync(Domain.Entities.DeliveryChallanHeader entity, int packedStatusId, int reservedStatusId, int typeId)
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

                            // Update StockLedger: Packed → Reserved (single range UPDATE)
                            if (reservedStatusId > 0)
                            {
                                await _dbContext.Database.ExecuteSqlRawAsync(
                                    "UPDATE Sales.StockLedger SET StatusId = {0} WHERE DocType = 'PROD' AND ItemId = {1} AND LotId = {2} AND PackNo >= {3} AND PackNo <= {4} AND StatusId = {5}",
                                    reservedStatusId, detail.ItemId, detail.LotId, detail.StartPackNo, detail.EndPackNo, packedStatusId);
                            }
                        }

                        await _dbContext.SaveChangesAsync();
                    }

                    // Increment DocNo in Finance.DocumentSequence
                    await _dbContext.Database.ExecuteSqlRawAsync(
                        "UPDATE [Finance].[DocumentSequence] SET DocNo = DocNo + 1 WHERE TransactionTypeId = {0} AND IsDeleted = 0",
                        typeId);

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

        public async Task<bool> SoftDeleteAsync(int id, int reservedStatusId, int packedStatusId, CancellationToken ct)
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

                    // Reverse StockLedger: Reserved → Packed (single range UPDATE per detail)
                    if (existing.DeliveryChallanDetails != null && existing.DeliveryChallanDetails.Count > 0)
                    {
                        foreach (var detail in existing.DeliveryChallanDetails)
                        {
                            await _dbContext.Database.ExecuteSqlRawAsync(
                                "UPDATE Sales.StockLedger SET StatusId = {0} WHERE DocType = 'PROD' AND ItemId = {1} AND LotId = {2} AND PackNo >= {3} AND PackNo <= {4} AND StatusId = {5}",
                                packedStatusId, detail.ItemId, detail.LotId, detail.StartPackNo, detail.EndPackNo, reservedStatusId);
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
