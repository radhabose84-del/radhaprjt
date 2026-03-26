using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Domain.Entities;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.DispatchAdvice
{
    public class DispatchAdviceCommandRepository : IDispatchAdviceCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;

        public DispatchAdviceCommandRepository(
            ApplicationDbContext applicationDbContext,
            IDocumentSequenceLookup documentSequenceLookup)
        {
            _applicationDbContext = applicationDbContext;
            _documentSequenceLookup = documentSequenceLookup;
        }

        public async Task<int> CreateAsync(DispatchAdviceHeader entity, int unitId, int packedStatusId, int reservedStatusId, int transactionTypeId)
        {
            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    // Separate details from header
                    var details = entity.DispatchAdviceDetails?.ToList();
                    entity.DispatchAdviceDetails = null;

                    // Insert header
                    await _applicationDbContext.DispatchAdviceHeader.AddAsync(entity);
                    await _applicationDbContext.SaveChangesAsync();

                    // Insert details and update StockLedger per detail line
                    if (details != null && details.Count > 0)
                    {
                        foreach (var detail in details)
                        {
                            detail.DispatchAdviceHeaderId = entity.Id;
                            await _applicationDbContext.DispatchAdviceDetail.AddAsync(detail);

                            // Update StockLedger: change each PackNo from Packed to Reserved
                            // No BETWEEN — update only records whose StatusId is still Packed (skip already invoiced)
                            for (int packNo = detail.StartPackNo; packNo <= detail.EndPackNo; packNo++)
                            {
                                var stockRecord = await _applicationDbContext.StockLedger
                                    .FirstOrDefaultAsync(s => s.UnitId == unitId
                                        && s.ItemId == detail.ItemId
                                        && s.LotId == detail.LotId
                                        && s.PackNo == packNo
                                        && s.PackTypeId == detail.PackTypeId
                                        && s.StatusId == packedStatusId);

                                if (stockRecord != null)
                                {
                                    stockRecord.StatusId = reservedStatusId;
                                }
                            }
                        }
                        await _applicationDbContext.SaveChangesAsync();
                    }

                    // Increment DocNo via lookup — same connection + transaction
                    var dbConnection = _applicationDbContext.Database.GetDbConnection();
                    var dbTransaction = transaction.GetDbTransaction();
                    await _documentSequenceLookup.IncrementDocNoAsync(transactionTypeId, dbConnection, dbTransaction);

                    await _applicationDbContext.SaveChangesAsync();
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

        public async Task<bool> SoftDeleteAsync(int id, int reservedStatusId, int packedStatusId, CancellationToken ct)
        {
            var existing = await _applicationDbContext.DispatchAdviceHeader
                .Include(h => h.DispatchAdviceDetails)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync(ct);
                try
                {
                    // Get UnitId from the linked SalesOrder
                    var unitId = await _applicationDbContext.SalesOrderHeader
                        .Where(s => s.Id == existing.SalesOrderId && s.IsDeleted == IsDelete.NotDeleted)
                        .Select(s => s.UnitId)
                        .FirstOrDefaultAsync(ct);

                    // Reverse StockLedger: change each PackNo from Reserved back to Packed
                    if (existing.DispatchAdviceDetails != null && existing.DispatchAdviceDetails.Count > 0)
                    {
                        foreach (var detail in existing.DispatchAdviceDetails)
                        {
                            for (int packNo = detail.StartPackNo; packNo <= detail.EndPackNo; packNo++)
                            {
                                var stockRecord = await _applicationDbContext.StockLedger
                                    .FirstOrDefaultAsync(s => s.UnitId == unitId
                                        && s.ItemId == detail.ItemId
                                        && s.LotId == detail.LotId
                                        && s.PackNo == packNo
                                        && s.PackTypeId == detail.PackTypeId
                                        && s.StatusId == reservedStatusId, ct);

                                if (stockRecord != null)
                                {
                                    stockRecord.StatusId = packedStatusId;
                                }
                            }
                        }
                    }

                    // Soft delete header
                    existing.IsDeleted = IsDelete.Deleted;
                    _applicationDbContext.DispatchAdviceHeader.Update(existing);
                    await _applicationDbContext.SaveChangesAsync(ct);

                    await transaction.CommitAsync(ct);
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync(ct);
                    throw;
                }
            });
        }

    }
}
