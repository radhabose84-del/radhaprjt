using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Updates.Party;
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
        private readonly IPartyFreightUpdate _partyFreightUpdate;

        public DispatchAdviceCommandRepository(
            ApplicationDbContext applicationDbContext,
            IDocumentSequenceLookup documentSequenceLookup,
            IPartyFreightUpdate partyFreightUpdate)
        {
            _applicationDbContext = applicationDbContext;
            _documentSequenceLookup = documentSequenceLookup;
            _partyFreightUpdate = partyFreightUpdate;
        }

        public async Task<int> CreateAsync(
            DispatchAdviceHeader entity,
            int unitId,
            int packedStatusId,
            int reservedStatusId,
            int transactionTypeId,
            string? dispatchTypeName,
            string directToPartyName,
            string othersName)
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

                    // Address-based freight propagation — runs INSIDE the same transaction
                    //   • Direct-To-Party → Party.PartyMaster.SalesFreightId (cross-module via IPartyFreightUpdate)
                    //   • Others          → Sales.DispatchAddressMaster.FreightId (same-module inline SQL)
                    var dbConnection = _applicationDbContext.Database.GetDbConnection();
                    var dbTransaction = transaction.GetDbTransaction();

                    // Propagation only when both: handler signaled it via dispatchTypeName AND a FreightId was actually provided.
                    // FreightId is null for non-Prepaid SalesOrders — nothing to propagate in that case.
                    if (entity.FreightId.HasValue)
                    {
                        if (string.Equals(dispatchTypeName, directToPartyName, StringComparison.OrdinalIgnoreCase))
                        {
                            await _partyFreightUpdate.UpdateSalesFreightIfNullAsync(
                                entity.PartyId, entity.FreightId.Value, dbConnection, dbTransaction);
                        }
                        else if (string.Equals(dispatchTypeName, othersName, StringComparison.OrdinalIgnoreCase)
                                 && entity.DispatchAddressId.HasValue)
                        {
                            const string addressSql = @"
                                UPDATE Sales.DispatchAddressMaster
                                SET FreightId = {0}
                                WHERE Id = {1} AND FreightId IS NULL";

                            await _applicationDbContext.Database.ExecuteSqlRawAsync(addressSql, entity.FreightId.Value, entity.DispatchAddressId.Value);
                        }
                    }

                    // Increment DocNo via lookup — same connection + transaction
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
