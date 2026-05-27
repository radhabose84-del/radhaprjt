using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;
using PurchaseManagement.Domain.Entities.PurchaseReturn;
using PurchaseManagement.Infrastructure.Data;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Repositories.PurchaseReturn;

public sealed class PurchaseReturnCommandRepository : IPurchaseReturnCommandRepository
{
    private readonly ApplicationDbContext _db;

    public PurchaseReturnCommandRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PurchaseReturnHeader> CreateAsync(PurchaseReturnHeader entity, CancellationToken ct)
    {
        var strategy = _db.Database.CreateExecutionStrategy();
        PurchaseReturnHeader saved = null!;

        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                _db.Set<PurchaseReturnHeader>().Add(entity);
                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                saved = entity;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        });

        return saved;
    }

    public async Task<PurchaseReturnHeader> UpdateAsync(PurchaseReturnHeader entity, CancellationToken ct)
    {
        var existing = await _db.Set<PurchaseReturnHeader>()
            .Include(h => h.Details)
            .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted, ct);

        if (existing is null)
            throw new KeyNotFoundException("Purchase Return not found.");

        // Header fields (RtvNumber + StatusId are NOT updatable)
        existing.RtvDate = entity.RtvDate;
        existing.UnitId = entity.UnitId;
        existing.VendorId = entity.VendorId;
        existing.PoId = entity.PoId;
        existing.GrnHeaderId = entity.GrnHeaderId;
        existing.ReturnTypeId = entity.ReturnTypeId;
        existing.ReturnReasonId = entity.ReturnReasonId;
        existing.ReturnActionId = entity.ReturnActionId;
        existing.IsReplacementRequired = entity.IsReplacementRequired;
        existing.IsDebitNoteRequired = entity.IsDebitNoteRequired;
        existing.IsQcVerified = entity.IsQcVerified;
        existing.Remarks = entity.Remarks;
        existing.IsActive = entity.IsActive;

        // Replace details: soft-delete missing, update kept, add new
        if (entity.Details != null)
        {
            var incomingIds = entity.Details.Where(d => d.Id > 0).Select(d => d.Id).ToHashSet();

            foreach (var existingLine in existing.Details)
            {
                if (!incomingIds.Contains(existingLine.Id))
                {
                    existingLine.IsDeleted = IsDelete.Deleted;
                    existingLine.IsActive = Status.Inactive;
                }
            }

            foreach (var incoming in entity.Details)
            {
                if (incoming.Id > 0)
                {
                    var existingLine = existing.Details.FirstOrDefault(d => d.Id == incoming.Id);
                    if (existingLine != null)
                    {
                        existingLine.GrnDetailId = incoming.GrnDetailId;
                        existingLine.ItemId = incoming.ItemId;
                        existingLine.UomId = incoming.UomId;
                        existingLine.ReceivedQty = incoming.ReceivedQty;
                        existingLine.AcceptedQty = incoming.AcceptedQty;
                        existingLine.ReturnQty = incoming.ReturnQty;
                        existingLine.RatePerUnit = incoming.RatePerUnit;
                        existingLine.LineValue = incoming.LineValue;
                        existingLine.ReturnReasonId = incoming.ReturnReasonId;
                        existingLine.LineRemarks = incoming.LineRemarks;
                    }
                }
                else
                {
                    incoming.PurchaseReturnHeaderId = existing.Id;
                    existing.Details.Add(incoming);
                }
            }
        }

        await _db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
    {
        var existing = await _db.Set<PurchaseReturnHeader>()
            .Include(h => h.Details)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

        if (existing is null)
            return false;

        existing.IsDeleted = IsDelete.Deleted;
        existing.IsActive = Status.Inactive;

        foreach (var line in existing.Details)
        {
            line.IsDeleted = IsDelete.Deleted;
            line.IsActive = Status.Inactive;
        }

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> SetStatusAsync(int id, int statusId, CancellationToken ct)
    {
        var existing = await _db.Set<PurchaseReturnHeader>()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);
        if (existing is null) return false;

        existing.StatusId = statusId;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> SetApprovalRequestIdAsync(int id, int approvalRequestId, CancellationToken ct)
    {
        var existing = await _db.Set<PurchaseReturnHeader>()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);
        if (existing is null) return false;

        existing.ApprovalRequestId = approvalRequestId;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task WriteStockLedgerOnApprovalAsync(int purchaseReturnHeaderId, CancellationToken ct)
    {
        // Insert one Purchase.StockLedger row per detail line.
        // DocType='RTV', IssueQty=ReturnQty, WarehouseId=GrnHeader.ReceivingWarehouseId
        const string sql = @"
            INSERT INTO Purchase.StockLedger
                (DocType, DocNo, DocSlNo, DocDate, ItemId, UomId, WarehouseId, StorageTypeId, TargetId,
                 ReceivedQty, ReceivedValue, IssueQty, IssueValue,
                 CreatedBy, CreatedDate, CreatedByName, CreatedIP,
                 IsActive, IsDeleted)
            SELECT
                'RTV', h.RtvNumber, d.Id, h.RtvDate,
                d.ItemId, d.UomId, gh.ReceivingWarehouseId, 0, 0,
                0, 0, d.ReturnQty, ISNULL(d.LineValue, 0),
                h.CreatedBy, SYSDATETIMEOFFSET(), h.CreatedByName, h.CreatedIP,
                1, 0
            FROM Purchase.PurchaseReturnDetail d
            INNER JOIN Purchase.PurchaseReturnHeader h ON h.Id = d.PurchaseReturnHeaderId
            INNER JOIN Purchase.GrnHeader gh ON gh.Id = h.GrnHeaderId
            WHERE d.PurchaseReturnHeaderId = @id
              AND d.IsDeleted = 0;";

        await _db.Database.ExecuteSqlRawAsync(sql,
            new Microsoft.Data.SqlClient.SqlParameter("@id", purchaseReturnHeaderId));
    }
}
