using System.Data.Common;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPO;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.ContractPOMaster;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ContractPO;
using PurchaseManagement.Infrastructure.Data;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.ContractPO;

public sealed class ContractPOCommandRepository : IContractPOCommandRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IDocumentSequenceLookup _documentSequenceLookup;
    private readonly IMiscMasterQueryRepository _misc;
    private readonly IIPAddressService _ipAddressService;

    public ContractPOCommandRepository(
        ApplicationDbContext db,
        IDocumentSequenceLookup documentSequenceLookup,
        IMiscMasterQueryRepository misc,
        IIPAddressService ipAddressService)
    {
        _db = db;
        _documentSequenceLookup = documentSequenceLookup;
        _misc = misc;
        _ipAddressService = ipAddressService;
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

    public async Task<int> UpdateContractPOAsync(
        PurchaseOrderHeader poHeader,
        PurchaseContractHeader contractHeader,
        List<PurchaseContractDetail> contractDetails,
        List<ContractPOReleaseHistory> releaseHistories,
        List<PurchasePaymentTerm> paymentTerms,
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

                // 6b) Delete-and-reinsert PaymentTerms (same pattern as Local PO)
                var existingPaymentTerms = await _db.Set<PurchasePaymentTerm>()
                    .Where(x => x.PurchaseOrderId == poHeader.Id)
                    .ToListAsync(ct);
                if (existingPaymentTerms.Count > 0)
                    _db.Set<PurchasePaymentTerm>().RemoveRange(existingPaymentTerms);

                foreach (var t in paymentTerms)
                {
                    t.PurchaseOrderId = existingPO.Id;
                    _db.Set<PurchasePaymentTerm>().Add(t);
                }

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

    public async Task<int> DeleteContractPOAsync(int poId, CancellationToken ct)
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

    public async Task<int> AmendContractPOAsync(
        int existingPoId,
        PurchaseOrderHeader newPoHeader,
        PurchaseContractHeader newContractHeader,
        List<PurchaseContractDetail> newContractDetails,
        List<ContractPOReleaseHistory> newReleaseHistories,
        List<PurchasePaymentTerm> newPaymentTerms,
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

                // 4b) Remove old payment terms
                var oldPaymentTerms = await _db.Set<PurchasePaymentTerm>()
                    .Where(x => x.PurchaseOrderId == existingPoId)
                    .ToListAsync(ct);
                if (oldPaymentTerms.Count > 0)
                    _db.Set<PurchasePaymentTerm>().RemoveRange(oldPaymentTerms);

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

                // 7b) Insert new PaymentTerms
                foreach (var t in newPaymentTerms)
                {
                    t.PurchaseOrderId = newPoHeader.Id;
                    _db.Set<PurchasePaymentTerm>().Add(t);
                }
                if (newPaymentTerms.Count > 0)
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

    // ── Shared Transaction overloads ────────────────────────────────────────

    public IExecutionStrategy CreateExecutionStrategy()
        => _db.Database.CreateExecutionStrategy();

    public async Task<(IDbContextTransaction EfTx, DbConnection Conn, DbTransaction DbTx)>
        BeginTransactionWithConnectionAsync(CancellationToken ct)
    {
        var conn = _db.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync(ct);

        var efTx = await _db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.ReadCommitted, ct);
        var dbTx = efTx.GetDbTransaction();
        return (efTx, conn, dbTx);
    }

    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);

    /// <summary>
    /// Creates PO + contract header/details + release history + updates balances
    /// WITHOUT managing its own transaction. Caller must manage the transaction.
    /// </summary>
    public async Task<int> CreateWithoutTransactionAsync(
        PurchaseOrderHeader poHeader,
        PurchaseContractHeader contractHeader,
        List<PurchaseContractDetail> contractDetails,
        List<ContractPOReleaseHistory> releaseHistories,
        List<PurchasePaymentTerm> paymentTerms,
        CancellationToken ct)
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

        // 3b) Insert PaymentTerms linked to PurchaseOrderHeader
        foreach (var t in paymentTerms)
        {
            t.PurchaseOrderId = poHeader.Id;
            _db.Set<PurchasePaymentTerm>().Add(t);
        }
        if (paymentTerms.Count > 0)
            await _db.SaveChangesAsync(ct);

        // 4) Update ContractPODetail balances
        await ApplyUtilizationAsync(contractDetails, contractHeader.ContractPOHeaderId, ct);

        return poHeader.Id;
    }

    /// <summary>
    /// Amends (reverse old + soft-close + create new) WITHOUT managing its own transaction.
    /// Caller must manage the transaction.
    /// </summary>
    public async Task<int> AmendWithoutTransactionAsync(
        int existingPoId,
        PurchaseOrderHeader newPoHeader,
        PurchaseContractHeader newContractHeader,
        List<PurchaseContractDetail> newContractDetails,
        List<ContractPOReleaseHistory> newReleaseHistories,
        List<PurchasePaymentTerm> newPaymentTerms,
        CancellationToken ct)
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

        // 4b) Remove old payment terms
        var oldPaymentTerms = await _db.Set<PurchasePaymentTerm>()
            .Where(x => x.PurchaseOrderId == existingPoId)
            .ToListAsync(ct);
        if (oldPaymentTerms.Count > 0)
            _db.Set<PurchasePaymentTerm>().RemoveRange(oldPaymentTerms);

        await _db.SaveChangesAsync(ct);

        // ---- CREATE NEW PO ----

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

        // 7b) Insert new PaymentTerms
        foreach (var t in newPaymentTerms)
        {
            t.PurchaseOrderId = newPoHeader.Id;
            _db.Set<PurchasePaymentTerm>().Add(t);
        }
        if (newPaymentTerms.Count > 0)
            await _db.SaveChangesAsync(ct);

        // 8) Apply new utilization
        await ApplyUtilizationAsync(newContractDetails, newContractHeader.ContractPOHeaderId, ct);

        return newPoHeader.Id;
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

    /* ========================= CANCEL ========================= */
    public async Task<bool> CancelAsync(int id, CancellationToken ct)
    {
        var existing = await _db.PurchaseOrderHeaders
            .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == BaseEntity.IsDelete.NotDeleted, ct);

        if (existing == null)
            return false;

        var cancelledStatus = await _misc.GetMiscMasterByName(
            MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Cancelled);
        existing.StatusId = cancelledStatus?.Id ?? existing.StatusId;

        existing.CancelledDate = DateTimeOffset.UtcNow;
        existing.CancelledByName = _ipAddressService.GetUserName();
        existing.CancelledIP = _ipAddressService.GetUserIPAddress();

        _db.PurchaseOrderHeaders.Update(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    /* ========================= FORECLOSE ========================= */
    public async Task<bool> ForecloseAsync(int id, CancellationToken ct)
    {
        var existing = await _db.PurchaseOrderHeaders
            .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == BaseEntity.IsDelete.NotDeleted, ct);

        if (existing == null)
            return false;

        var foreclosedStatus = await _misc.GetMiscMasterByName(
            MiscEnumEntity.ApprovalStatus, MiscEnumEntity.ForeClosed);
        existing.StatusId = foreclosedStatus?.Id ?? existing.StatusId;

        existing.ForeClosedDate = DateTimeOffset.UtcNow;
        existing.ForeClosedByName = _ipAddressService.GetUserName();
        existing.ForeClosedIP = _ipAddressService.GetUserIPAddress();

        _db.PurchaseOrderHeaders.Update(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
