using Contracts.Interfaces;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Application.Common.Interfaces.IReturnReason;
using PurchaseManagement.Infrastructure.Data;
using static PurchaseManagement.Domain.Common.BaseEntity;
using DomainReturnReason = PurchaseManagement.Domain.Entities.PurchaseReturn.ReturnReason;

namespace PurchaseManagement.Infrastructure.Repositories.PurchaseReturn;

public sealed class ReturnReasonCommandRepository : IReturnReasonCommandRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IIPAddressService _ip;

    public ReturnReasonCommandRepository(ApplicationDbContext db, IIPAddressService ip)
    {
        _db = db;
        _ip = ip;
    }

    public async Task<DomainReturnReason> CreateAsync(DomainReturnReason entity, CancellationToken ct)
    {
        entity.Code = entity.Code.Trim();
        entity.Description = entity.Description.Trim();

        _db.Set<DomainReturnReason>().Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<DomainReturnReason> UpdateAsync(DomainReturnReason entity, CancellationToken ct)
    {
        var existing = await _db.Set<DomainReturnReason>()
            .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted, ct);

        if (existing is null)
            throw new KeyNotFoundException("Return Reason not found.");

        // Code is immutable per spec
        existing.Description = entity.Description.Trim();
        existing.ReturnTypeId = entity.ReturnTypeId;
        existing.IsReplacementOverride = entity.IsReplacementOverride;
        existing.IsDebitNoteOverride = entity.IsDebitNoteOverride;
        existing.IsQcMandatoryOverride = entity.IsQcMandatoryOverride;
        existing.IsActive = entity.IsActive;

        await _db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
    {
        var existing = await _db.Set<DomainReturnReason>()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

        if (existing is null)
            return false;

        existing.IsDeleted = IsDelete.Deleted;
        existing.IsActive = Status.Inactive;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
