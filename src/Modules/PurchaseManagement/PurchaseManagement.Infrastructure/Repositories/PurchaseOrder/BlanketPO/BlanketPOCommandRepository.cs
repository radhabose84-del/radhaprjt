using System.Data.Common;
using Contracts.Common;
using Contracts.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IBlanketPO;
using PurchaseManagement.Application.PurchaseOrder.BlanketPO.Commands.Create;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.Entities.PurchaseOrder.BlanketPO;
using PurchaseManagement.Infrastructure.Data;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.BlanketPO;

public sealed class BlanketPOCommandRepository : IBlanketPOCommandRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IMiscMasterQueryRepository _misc;
    private readonly IIPAddressService _ipAddressService;

    public BlanketPOCommandRepository(
        ApplicationDbContext db,
        IMiscMasterQueryRepository misc,
        IIPAddressService ipAddressService)
    {
        _db = db;
        _misc = misc;
        _ipAddressService = ipAddressService;
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
    /// Creates PO + blanket header/details + payment terms
    /// WITHOUT managing its own transaction. Caller must manage the transaction.
    /// </summary>
    public async Task<int> CreateWithoutTransactionAsync(
        PurchaseOrderHeader poHeader,
        PurchaseBlanketHeader blanketHeader,
        List<PurchaseBlanketDetail> blanketDetails,
        List<PurchasePaymentTerm> paymentTerms,
        CancellationToken ct)
    {
        // 1) Insert PurchaseOrderHeader
        _db.Set<PurchaseOrderHeader>().Add(poHeader);
        await _db.SaveChangesAsync(ct);

        // 2) Insert PurchaseBlanketHeader linked to the PO
        blanketHeader.PurchaseOrderId = poHeader.Id;
        _db.Set<PurchaseBlanketHeader>().Add(blanketHeader);
        await _db.SaveChangesAsync(ct);

        // 3) Insert PurchaseBlanketDetail lines
        foreach (var detail in blanketDetails)
        {
            detail.PurchaseBlanketHeaderId = blanketHeader.Id;
            _db.Set<PurchaseBlanketDetail>().Add(detail);
        }
        await _db.SaveChangesAsync(ct);

        // 4) Insert PaymentTerms linked to PurchaseOrderHeader
        foreach (var t in paymentTerms)
        {
            t.PurchaseOrderId = poHeader.Id;
            _db.Set<PurchasePaymentTerm>().Add(t);
        }
        if (paymentTerms.Count > 0)
            await _db.SaveChangesAsync(ct);

        return poHeader.Id;
    }

    /// <summary>
    /// Updates an existing Blanket Release PO (PO header + blanket header + details + payment terms).
    /// Uses a transaction internally.
    /// </summary>
    public async Task<int> UpdateBlanketPOAsync(
        PurchaseOrderHeader poHeader,
        PurchaseBlanketHeader blanketHeader,
        List<PurchaseBlanketDetail> blanketDetails,
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
                // 1) Load existing PO + blanket header + details
                var existingPO = await _db.Set<PurchaseOrderHeader>()
                    .FirstOrDefaultAsync(x => x.Id == poHeader.Id && x.IsDeleted == IsDelete.NotDeleted, ct)
                    ?? throw new ExceptionRules("Blanket Release PO not found.");

                var existingBlanketHeader = await _db.Set<PurchaseBlanketHeader>()
                    .FirstOrDefaultAsync(x => x.PurchaseOrderId == poHeader.Id && x.IsDeleted == IsDelete.NotDeleted, ct)
                    ?? throw new ExceptionRules("Blanket header not found for this PO.");

                var existingDetails = await _db.Set<PurchaseBlanketDetail>()
                    .Where(x => x.PurchaseBlanketHeaderId == existingBlanketHeader.Id && x.IsDeleted == IsDelete.NotDeleted)
                    .ToListAsync(ct);

                // 2) Soft-delete old blanket details
                foreach (var d in existingDetails)
                {
                    d.IsDeleted = IsDelete.Deleted;
                    d.IsActive = Status.Inactive;
                }
                await _db.SaveChangesAsync(ct);

                // 3) Update PurchaseOrderHeader fields
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
                existingPO.CostCenterId = poHeader.CostCenterId;
                existingPO.BudgetGroupId = poHeader.BudgetGroupId;
                existingPO.BudgetMonthId = poHeader.BudgetMonthId;
                existingPO.BudgetRequestById = poHeader.BudgetRequestById;
                existingPO.ProjectId = poHeader.ProjectId;
                existingPO.WBSId = poHeader.WBSId;
                existingPO.FinancialYearId = poHeader.FinancialYearId;

                // 4) Update PurchaseBlanketHeader fields
                existingBlanketHeader.BlanketHeaderId = blanketHeader.BlanketHeaderId;
                existingBlanketHeader.IsPartialReceiptAllowed = blanketHeader.IsPartialReceiptAllowed;
                existingBlanketHeader.IncotermsId = blanketHeader.IncotermsId;
                existingBlanketHeader.ModeOfDispatchId = blanketHeader.ModeOfDispatchId;
                existingBlanketHeader.FreightCharges = blanketHeader.FreightCharges;
                existingBlanketHeader.TermsId = blanketHeader.TermsId;
                existingBlanketHeader.TermDescription = blanketHeader.TermDescription;
                existingBlanketHeader.DeliveryAddress = blanketHeader.DeliveryAddress;
                existingBlanketHeader.BillingAddress = blanketHeader.BillingAddress;

                // 5) Delete-and-reinsert PaymentTerms
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

                // 6) Insert new blanket details
                foreach (var detail in blanketDetails)
                {
                    detail.PurchaseBlanketHeaderId = existingBlanketHeader.Id;
                    _db.Set<PurchaseBlanketDetail>().Add(detail);
                }
                await _db.SaveChangesAsync(ct);

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

    /* ========================= CANCEL ========================= */
    public async Task<bool> CancelAsync(int id, CancellationToken ct)
    {
        var existing = await _db.PurchaseOrderHeaders
            .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

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

    /* ========================= WORKFLOW ========================= */
    public async Task<BlanketPOWorkFlowDto> GetByIdBlanketPOWorkFlowAsync(int id)
    {
        var entity = await _db.Set<PurchaseOrderHeader>()
            .Where(x => x.Id == id)
            .Select(x => new BlanketPOWorkFlowDto
            {
                Id = x.Id,
                PONumber = x.PONumber,
                VendorId = x.VendorId,
                StatusId = x.StatusId,
                UnitId = x.UnitId
            })
            .FirstOrDefaultAsync();
        return entity!;
    }

    /* ========================= FORECLOSE ========================= */
    public async Task<bool> ForecloseAsync(int id, CancellationToken ct)
    {
        var existing = await _db.PurchaseOrderHeaders
            .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

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
