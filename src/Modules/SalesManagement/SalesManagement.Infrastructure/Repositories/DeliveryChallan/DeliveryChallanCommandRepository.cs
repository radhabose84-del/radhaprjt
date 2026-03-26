using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Domain.Common;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.DeliveryChallan
{
    public class DeliveryChallanCommandRepository : IDeliveryChallanCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;

        public DeliveryChallanCommandRepository(
            ApplicationDbContext dbContext,
            IDocumentSequenceLookup documentSequenceLookup)
        {
            _dbContext = dbContext;
            _documentSequenceLookup = documentSequenceLookup;
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

        public async Task UpdateApprovalStatusAsync(int id, string status, CancellationToken ct)
        {
            var existing = await _dbContext.DeliveryChallanHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return;

            var statusId = await _dbContext.MiscMaster
                .Where(m => m.IsDeleted == IsDelete.NotDeleted
                    && m.IsActive == Status.Active
                    && m.MiscTypeMaster != null
                    && m.MiscTypeMaster.IsDeleted == IsDelete.NotDeleted
                    && m.MiscTypeMaster.MiscTypeCode == MiscEnumEntity.StoApprovalStatus
                    && m.Code == status)
                .Select(m => (int?)m.Id)
                .FirstOrDefaultAsync(ct);

            if (statusId.HasValue)
            {
                existing.StatusId = statusId.Value;
                _dbContext.DeliveryChallanHeader.Update(existing);
                await _dbContext.SaveChangesAsync(ct);
            }
        }
    }
}
