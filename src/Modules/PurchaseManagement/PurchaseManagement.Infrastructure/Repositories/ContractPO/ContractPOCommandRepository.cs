using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PurchaseManagement.Application.Common.Interfaces.IContractPO;
using PurchaseManagement.Domain.Entities.ContractPO;
using PurchaseManagement.Infrastructure.Data;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Repositories.ContractPO;

public sealed class ContractPOCommandRepository : IContractPOCommandRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IIPAddressService _ipAddressService;
    private readonly IDocumentSequenceLookup _documentSequenceLookup;

    public ContractPOCommandRepository(
        ApplicationDbContext db,
        IIPAddressService ipAddressService,
        IDocumentSequenceLookup documentSequenceLookup)
    {
        _db = db;
        _ipAddressService = ipAddressService;
        _documentSequenceLookup = documentSequenceLookup;
    }

    public async Task<ContractPOHeader> CreateAsync(ContractPOHeader entity, int transactionTypeId, CancellationToken ct)
    {
        var strategy = _db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.ReadCommitted, ct);

            try
            {
                // ContractPONumber already assigned by handler via GenerateDocumentNumber
                _db.Set<ContractPOHeader>().Add(entity);
                await _db.SaveChangesAsync(ct);

                // Increment DocNo via lookup — same connection + transaction
                var dbConnection = _db.Database.GetDbConnection();
                var dbTransaction = tx.GetDbTransaction();
                await _documentSequenceLookup.IncrementDocNoAsync(transactionTypeId, dbConnection, dbTransaction);

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return entity;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        });
    }

    public async Task<ContractPOHeader> UpdateAsync(
        ContractPOHeader entity, List<ContractPODetail> details, CancellationToken ct)
    {
        var strategy = _db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.ReadCommitted, ct);

            var existing = await _db.Set<ContractPOHeader>()
                .Include(h => h.ContractPODetails)
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted, ct)
                ?? throw new ExceptionRules("Contract PO not found.");

            // Update header fields (ContractPONumber and ContractDate are immutable)
            existing.VendorId = entity.VendorId;
            existing.CurrencyId = entity.CurrencyId;
            existing.ValidityFrom = entity.ValidityFrom;
            existing.ValidityTo = entity.ValidityTo;
            existing.StatusId = entity.StatusId;
            existing.Remarks = entity.Remarks;
            existing.IsActive = entity.IsActive;

            // Process details: identify new, updated, removed
            var incomingIds = details.Where(d => d.Id > 0).Select(d => d.Id).ToHashSet();
            var existingDetails = existing.ContractPODetails.ToList();

            // Remove details not in the incoming list
            foreach (var existingDetail in existingDetails)
            {
                if (!incomingIds.Contains(existingDetail.Id))
                {
                    existingDetail.IsDeleted = IsDelete.Deleted;
                    existingDetail.IsActive = Status.Inactive;
                }
            }

            // Update or add details
            foreach (var detail in details)
            {
                if (detail.Id > 0)
                {
                    // Update existing detail
                    var existingDetail = existingDetails.FirstOrDefault(d => d.Id == detail.Id);
                    if (existingDetail != null)
                    {
                        existingDetail.ItemSno = detail.ItemSno;
                        existingDetail.ItemId = detail.ItemId;
                        existingDetail.UOMId = detail.UOMId;
                        existingDetail.ContractQuantity = detail.ContractQuantity;
                        existingDetail.ContractRate = detail.ContractRate;
                        existingDetail.ContractValue = detail.ContractQuantity * detail.ContractRate;
                        existingDetail.HSNId = detail.HSNId;
                        existingDetail.GSTPercentage = detail.GSTPercentage;
                        // Recalculate balance based on utilized
                        existingDetail.BalanceQuantity = detail.ContractQuantity - existingDetail.UtilizedQuantity;
                        existingDetail.BalanceValue = existingDetail.ContractValue - existingDetail.UtilizedValue;
                    }
                }
                else
                {
                    // New detail line
                    var newDetail = new ContractPODetail
                    {
                        ContractPOHeaderId = existing.Id,
                        ItemSno = detail.ItemSno,
                        ItemId = detail.ItemId,
                        UOMId = detail.UOMId,
                        ContractQuantity = detail.ContractQuantity,
                        ContractRate = detail.ContractRate,
                        ContractValue = detail.ContractQuantity * detail.ContractRate,
                        UtilizedQuantity = 0,
                        BalanceQuantity = detail.ContractQuantity,
                        UtilizedValue = 0,
                        BalanceValue = detail.ContractQuantity * detail.ContractRate,
                        HSNId = detail.HSNId,
                        GSTPercentage = detail.GSTPercentage
                    };
                    existing.ContractPODetails.Add(newDetail);
                }
            }

            // Recalculate header totals from active details
            var activeDetails = existing.ContractPODetails
                .Where(d => d.IsDeleted == IsDelete.NotDeleted).ToList();
            existing.TotalContractValue = activeDetails.Sum(d => d.ContractValue);
            existing.UtilizedValue = activeDetails.Sum(d => d.UtilizedValue);
            existing.BalanceValue = existing.TotalContractValue - existing.UtilizedValue;

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return existing;
        });
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
    {
        var existing = await _db.Set<ContractPOHeader>()
            .Include(h => h.ContractPODetails)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

        if (existing is null)
            return false;

        existing.IsDeleted = IsDelete.Deleted;
        existing.IsActive = Status.Inactive;
        existing.ModifiedBy = _ipAddressService.GetUserId();
        existing.ModifiedByName = _ipAddressService.GetUserName();
        existing.ModifiedIP = _ipAddressService.GetUserIPAddress();
        existing.ModifiedDate = DateTimeOffset.UtcNow;

        // Soft delete all details
        foreach (var detail in existing.ContractPODetails)
        {
            detail.IsDeleted = IsDelete.Deleted;
            detail.IsActive = Status.Inactive;
        }

        await _db.SaveChangesAsync(ct);
        return true;
    }
}
