using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPO;
using PurchaseManagement.Domain.Entities.ContractPO;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ContractPO;
using PurchaseManagement.Infrastructure.Data;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.ContractPO
{
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

        public async Task<int> CreateCombinePOAsync(
            PurchaseOrderHeader poHeader,
            PurchaseContractHeader contractHeader,
            List<PurchaseContractDetail> contractDetails,
            List<ContractPOReleaseHistory> releaseHistories,
            int transactionTypeId,
            CancellationToken ct)
        {
            var strategy = _db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.ReadCommitted, ct);

                try
                {
                    // 1) Insert PurchaseOrderHeader
                    _db.Set<PurchaseOrderHeader>().Add(poHeader);
                    await _db.SaveChangesAsync(ct);

                    // 2) Insert PurchaseContractHeader linked to the PO
                    contractHeader.PurchaseOrderId = poHeader.Id;
                    _db.Set<PurchaseContractHeader>().Add(contractHeader);
                    await _db.SaveChangesAsync(ct);

                    // 3) Insert PurchaseContractDetail lines + ContractPOReleaseHistory
                    for (var i = 0; i < contractDetails.Count; i++)
                    {
                        var detail = contractDetails[i];
                        detail.PurchaseContractHeaderId = contractHeader.Id;
                        _db.Set<PurchaseContractDetail>().Add(detail);

                        var history = releaseHistories[i];
                        history.ReleasePOId = poHeader.Id;
                        _db.Set<ContractPOReleaseHistory>().Add(history);
                    }
                    await _db.SaveChangesAsync(ct);

                    // 4) Update ContractPODetail balances
                    foreach (var detail in contractDetails)
                    {
                        var contractLine = await _db.Set<ContractPODetail>()
                            .FirstOrDefaultAsync(d => d.Id == detail.ContractPODetailId
                                && d.IsDeleted == IsDelete.NotDeleted, ct);

                        if (contractLine != null)
                        {
                            contractLine.UtilizedQuantity += detail.Quantity;
                            contractLine.BalanceQuantity = contractLine.ContractQuantity - contractLine.UtilizedQuantity;
                            contractLine.UtilizedValue += detail.ItemValue;
                            contractLine.BalanceValue = contractLine.ContractValue - contractLine.UtilizedValue;
                        }
                    }
                    await _db.SaveChangesAsync(ct);

                    // 5) Update ContractPOHeader totals
                    var contractPOHeader = await _db.Set<ContractPOHeader>()
                        .Include(h => h.ContractPODetails)
                        .FirstOrDefaultAsync(h => h.Id == contractHeader.ContractPOHeaderId
                            && h.IsDeleted == IsDelete.NotDeleted, ct);

                    if (contractPOHeader != null)
                    {
                        var activeLines = contractPOHeader.ContractPODetails
                            .Where(d => d.IsDeleted == IsDelete.NotDeleted).ToList();
                        contractPOHeader.UtilizedValue = activeLines.Sum(d => d.UtilizedValue);
                        contractPOHeader.BalanceValue = contractPOHeader.TotalContractValue - contractPOHeader.UtilizedValue;
                    }
                    await _db.SaveChangesAsync(ct);

                    // 6) Increment DocNo via lookup — same connection + transaction
                    var dbConnection = _db.Database.GetDbConnection();
                    var dbTransaction = tx.GetDbTransaction();
                    await _documentSequenceLookup.IncrementDocNoAsync(transactionTypeId, dbConnection, dbTransaction);

                    await _db.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);

                    return poHeader.Id;
                }
                catch
                {
                    await tx.RollbackAsync(ct);
                    throw;
                }
            });
        }

        public async Task<int> UpdateContractReleasePOAsync(
            PurchaseOrderHeader poHeader,
            PurchaseContractHeader contractHeader,
            List<PurchaseContractDetail> contractDetails,
            List<ContractPOReleaseHistory> releaseHistories,
            CancellationToken ct)
        {
            var strategy = _db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.ReadCommitted, ct);

                try
                {
                    // 1) Load existing PO + contract header + details
                    var existingPO = await _db.Set<PurchaseOrderHeader>()
                        .FirstOrDefaultAsync(x => x.Id == poHeader.Id && x.IsDeleted == IsDelete.NotDeleted, ct)
                        ?? throw new ExceptionRules("Contract Release PO not found.");

                    var existingContractHeader = await _db.Set<PurchaseContractHeader>()
                        .FirstOrDefaultAsync(x => x.PurchaseOrderId == poHeader.Id && x.IsDeleted == IsDelete.NotDeleted, ct)
                        ?? throw new ExceptionRules("Contract header not found for this PO.");

                    var existingDetails = await _db.Set<PurchaseContractDetail>()
                        .Where(x => x.PurchaseContractHeaderId == existingContractHeader.Id && x.IsDeleted == IsDelete.NotDeleted)
                        .ToListAsync(ct);

                    // 2) Reverse old utilization from ContractPODetail
                    await ReverseUtilizationAsync(existingDetails, existingContractHeader.ContractPOHeaderId, ct);

                    // 3) Soft-delete old release histories for this PO
                    var oldHistories = await _db.Set<ContractPOReleaseHistory>()
                        .Where(x => x.ReleasePOId == poHeader.Id && x.IsDeleted == IsDelete.NotDeleted)
                        .ToListAsync(ct);
                    foreach (var h in oldHistories)
                    {
                        h.IsDeleted = IsDelete.Deleted;
                        h.IsActive = Status.Inactive;
                    }

                    // 4) Soft-delete old contract details
                    foreach (var d in existingDetails)
                    {
                        d.IsDeleted = IsDelete.Deleted;
                        d.IsActive = Status.Inactive;
                    }
                    await _db.SaveChangesAsync(ct);

                    // 5) Update PurchaseOrderHeader fields
                    existingPO.UnitId = poHeader.UnitId;
                    existingPO.PODate = poHeader.PODate;
                    existingPO.POCategoryId = poHeader.POCategoryId;
                    existingPO.POMethodId = poHeader.POMethodId;
                    existingPO.CurrencyId = poHeader.CurrencyId;
                    existingPO.VendorId = poHeader.VendorId;
                    existingPO.ItemTotal = poHeader.ItemTotal;
                    existingPO.DiscountTotal = poHeader.DiscountTotal;
                    existingPO.PandFTotal = poHeader.PandFTotal;
                    existingPO.MiscCharges = poHeader.MiscCharges;
                    existingPO.GSTTotal = poHeader.GSTTotal;
                    existingPO.CGSTTotal = poHeader.CGSTTotal;
                    existingPO.SGSTTotal = poHeader.SGSTTotal;
                    existingPO.IGSTTotal = poHeader.IGSTTotal;
                    existingPO.FreightTotal = poHeader.FreightTotal;
                    existingPO.PurchaseValue = poHeader.PurchaseValue;
                    existingPO.StatusId = poHeader.StatusId;

                    // 6) Update PurchaseContractHeader fields
                    existingContractHeader.ContractPOHeaderId = contractHeader.ContractPOHeaderId;
                    existingContractHeader.IsPartialReceiptAllowed = contractHeader.IsPartialReceiptAllowed;
                    existingContractHeader.IncotermsId = contractHeader.IncotermsId;
                    existingContractHeader.ModeOfDispatchId = contractHeader.ModeOfDispatchId;
                    existingContractHeader.FreightCharges = contractHeader.FreightCharges;
                    existingContractHeader.TermsId = contractHeader.TermsId;
                    existingContractHeader.TermDescription = contractHeader.TermDescription;
                    existingContractHeader.DeliveryAddress = contractHeader.DeliveryAddress;
                    existingContractHeader.BillingAddress = contractHeader.BillingAddress;
                    await _db.SaveChangesAsync(ct);

                    // 7) Insert new contract details + release histories
                    for (var i = 0; i < contractDetails.Count; i++)
                    {
                        var detail = contractDetails[i];
                        detail.PurchaseContractHeaderId = existingContractHeader.Id;
                        _db.Set<PurchaseContractDetail>().Add(detail);

                        var history = releaseHistories[i];
                        history.ReleasePOId = poHeader.Id;
                        _db.Set<ContractPOReleaseHistory>().Add(history);
                    }
                    await _db.SaveChangesAsync(ct);

                    // 8) Apply new utilization to ContractPODetail
                    await ApplyUtilizationAsync(contractDetails, contractHeader.ContractPOHeaderId, ct);

                    await tx.CommitAsync(ct);
                    return existingPO.Id;
                }
                catch
                {
                    await tx.RollbackAsync(ct);
                    throw;
                }
            });
        }

        public async Task<int> DeleteContractReleasePOAsync(int poId, CancellationToken ct)
        {
            var strategy = _db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.ReadCommitted, ct);

                try
                {
                    // 1) Load existing PO
                    var existingPO = await _db.Set<PurchaseOrderHeader>()
                        .FirstOrDefaultAsync(x => x.Id == poId && x.IsDeleted == IsDelete.NotDeleted, ct);

                    if (existingPO is null)
                        return 0;

                    // 2) Load contract header
                    var contractHeader = await _db.Set<PurchaseContractHeader>()
                        .FirstOrDefaultAsync(x => x.PurchaseOrderId == poId && x.IsDeleted == IsDelete.NotDeleted, ct);

                    if (contractHeader is null)
                        return 0;

                    // 3) Load contract details
                    var contractDetails = await _db.Set<PurchaseContractDetail>()
                        .Where(x => x.PurchaseContractHeaderId == contractHeader.Id && x.IsDeleted == IsDelete.NotDeleted)
                        .ToListAsync(ct);

                    // 4) Reverse utilization from ContractPODetail
                    await ReverseUtilizationAsync(contractDetails, contractHeader.ContractPOHeaderId, ct);

                    // 5) Soft-delete release histories
                    var histories = await _db.Set<ContractPOReleaseHistory>()
                        .Where(x => x.ReleasePOId == poId && x.IsDeleted == IsDelete.NotDeleted)
                        .ToListAsync(ct);
                    foreach (var h in histories)
                    {
                        h.IsDeleted = IsDelete.Deleted;
                        h.IsActive = Status.Inactive;
                    }

                    // 6) Soft-delete contract details
                    foreach (var d in contractDetails)
                    {
                        d.IsDeleted = IsDelete.Deleted;
                        d.IsActive = Status.Inactive;
                    }

                    // 7) Soft-delete contract header
                    contractHeader.IsDeleted = IsDelete.Deleted;
                    contractHeader.IsActive = Status.Inactive;

                    // 8) Soft-delete PO header
                    existingPO.IsDeleted = IsDelete.Deleted;
                    existingPO.IsActive = Status.Inactive;

                    await _db.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);

                    return poId;
                }
                catch
                {
                    await tx.RollbackAsync(ct);
                    throw;
                }
            });
        }

        public async Task<int> AmendContractReleasePOAsync(
            int existingPoId,
            PurchaseOrderHeader newPoHeader,
            PurchaseContractHeader newContractHeader,
            List<PurchaseContractDetail> newContractDetails,
            List<ContractPOReleaseHistory> newReleaseHistories,
            int transactionTypeId,
            CancellationToken ct)
        {
            var strategy = _db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.ReadCommitted, ct);

                try
                {
                    // ---- CLOSE OLD PO ----

                    // 1) Load old PO + contract header + details
                    var oldPO = await _db.Set<PurchaseOrderHeader>()
                        .FirstOrDefaultAsync(x => x.Id == existingPoId && x.IsDeleted == IsDelete.NotDeleted, ct)
                        ?? throw new ExceptionRules("Existing Contract Release PO not found.");

                    var oldContractHeader = await _db.Set<PurchaseContractHeader>()
                        .FirstOrDefaultAsync(x => x.PurchaseOrderId == existingPoId && x.IsDeleted == IsDelete.NotDeleted, ct)
                        ?? throw new ExceptionRules("Existing contract header not found.");

                    var oldDetails = await _db.Set<PurchaseContractDetail>()
                        .Where(x => x.PurchaseContractHeaderId == oldContractHeader.Id && x.IsDeleted == IsDelete.NotDeleted)
                        .ToListAsync(ct);

                    // 2) Reverse old utilization
                    await ReverseUtilizationAsync(oldDetails, oldContractHeader.ContractPOHeaderId, ct);

                    // 3) Soft-delete old release histories
                    var oldHistories = await _db.Set<ContractPOReleaseHistory>()
                        .Where(x => x.ReleasePOId == existingPoId && x.IsDeleted == IsDelete.NotDeleted)
                        .ToListAsync(ct);
                    foreach (var h in oldHistories)
                    {
                        h.IsDeleted = IsDelete.Deleted;
                        h.IsActive = Status.Inactive;
                    }

                    // 4) Soft-close old details, contract header, and PO header
                    foreach (var d in oldDetails)
                    {
                        d.IsDeleted = IsDelete.Deleted;
                        d.IsActive = Status.Inactive;
                    }
                    oldContractHeader.IsDeleted = IsDelete.Deleted;
                    oldContractHeader.IsActive = Status.Inactive;
                    oldPO.IsDeleted = IsDelete.Deleted;
                    oldPO.IsActive = Status.Inactive;
                    await _db.SaveChangesAsync(ct);

                    // ---- CREATE NEW PO (same as CreateCombinePOAsync) ----

                    // 5) Insert new PurchaseOrderHeader
                    _db.Set<PurchaseOrderHeader>().Add(newPoHeader);
                    await _db.SaveChangesAsync(ct);

                    // 6) Insert new PurchaseContractHeader
                    newContractHeader.PurchaseOrderId = newPoHeader.Id;
                    _db.Set<PurchaseContractHeader>().Add(newContractHeader);
                    await _db.SaveChangesAsync(ct);

                    // 7) Insert new contract details + release histories
                    for (var i = 0; i < newContractDetails.Count; i++)
                    {
                        var detail = newContractDetails[i];
                        detail.PurchaseContractHeaderId = newContractHeader.Id;
                        _db.Set<PurchaseContractDetail>().Add(detail);

                        var history = newReleaseHistories[i];
                        history.ReleasePOId = newPoHeader.Id;
                        _db.Set<ContractPOReleaseHistory>().Add(history);
                    }
                    await _db.SaveChangesAsync(ct);

                    // 8) Apply new utilization
                    await ApplyUtilizationAsync(newContractDetails, newContractHeader.ContractPOHeaderId, ct);

                    // 9) Increment DocNo
                    var dbConnection = _db.Database.GetDbConnection();
                    var dbTransaction = tx.GetDbTransaction();
                    await _documentSequenceLookup.IncrementDocNoAsync(transactionTypeId, dbConnection, dbTransaction);

                    await _db.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);

                    return newPoHeader.Id;
                }
                catch
                {
                    await tx.RollbackAsync(ct);
                    throw;
                }
            });
        }

        // ---- Private helpers for contract balance management ----

        private async Task ReverseUtilizationAsync(
            List<PurchaseContractDetail> details, int contractPOHeaderId, CancellationToken ct)
        {
            foreach (var detail in details)
            {
                var contractLine = await _db.Set<ContractPODetail>()
                    .FirstOrDefaultAsync(d => d.Id == detail.ContractPODetailId
                        && d.IsDeleted == IsDelete.NotDeleted, ct);

                if (contractLine != null)
                {
                    contractLine.UtilizedQuantity -= detail.Quantity;
                    contractLine.BalanceQuantity = contractLine.ContractQuantity - contractLine.UtilizedQuantity;
                    contractLine.UtilizedValue -= detail.ItemValue;
                    contractLine.BalanceValue = contractLine.ContractValue - contractLine.UtilizedValue;
                }
            }
            await _db.SaveChangesAsync(ct);

            // Recalculate ContractPOHeader totals
            await RecalculateContractHeaderTotalsAsync(contractPOHeaderId, ct);
        }

        private async Task ApplyUtilizationAsync(
            List<PurchaseContractDetail> details, int contractPOHeaderId, CancellationToken ct)
        {
            foreach (var detail in details)
            {
                var contractLine = await _db.Set<ContractPODetail>()
                    .FirstOrDefaultAsync(d => d.Id == detail.ContractPODetailId
                        && d.IsDeleted == IsDelete.NotDeleted, ct);

                if (contractLine != null)
                {
                    contractLine.UtilizedQuantity += detail.Quantity;
                    contractLine.BalanceQuantity = contractLine.ContractQuantity - contractLine.UtilizedQuantity;
                    contractLine.UtilizedValue += detail.ItemValue;
                    contractLine.BalanceValue = contractLine.ContractValue - contractLine.UtilizedValue;
                }
            }
            await _db.SaveChangesAsync(ct);

            // Recalculate ContractPOHeader totals
            await RecalculateContractHeaderTotalsAsync(contractPOHeaderId, ct);
        }

        private async Task RecalculateContractHeaderTotalsAsync(int contractPOHeaderId, CancellationToken ct)
        {
            var contractPOHeader = await _db.Set<ContractPOHeader>()
                .Include(h => h.ContractPODetails)
                .FirstOrDefaultAsync(h => h.Id == contractPOHeaderId
                    && h.IsDeleted == IsDelete.NotDeleted, ct);

            if (contractPOHeader != null)
            {
                var activeLines = contractPOHeader.ContractPODetails
                    .Where(d => d.IsDeleted == IsDelete.NotDeleted).ToList();
                contractPOHeader.UtilizedValue = activeLines.Sum(d => d.UtilizedValue);
                contractPOHeader.BalanceValue = contractPOHeader.TotalContractValue - contractPOHeader.UtilizedValue;
                await _db.SaveChangesAsync(ct);
            }
        }
    }
}