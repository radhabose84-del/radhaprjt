using Contracts.Interfaces;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Application.Common.Interfaces.IReturnType;
using PurchaseManagement.Infrastructure.Data;
using static PurchaseManagement.Domain.Common.BaseEntity;
using DomainReturnType = PurchaseManagement.Domain.Entities.PurchaseReturn.ReturnType;

namespace PurchaseManagement.Infrastructure.Repositories.PurchaseReturn;

public sealed class ReturnTypeCommandRepository : IReturnTypeCommandRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IIPAddressService _ip;

    public ReturnTypeCommandRepository(ApplicationDbContext db, IIPAddressService ip)
    {
        _db = db;
        _ip = ip;
    }

    public async Task<DomainReturnType> CreateAsync(DomainReturnType entity, CancellationToken ct)
    {
        entity.Code = entity.Code.Trim();
        entity.Description = entity.Description.Trim();

        _db.Set<DomainReturnType>().Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<DomainReturnType> UpdateAsync(DomainReturnType entity, CancellationToken ct)
    {
        var existing = await _db.Set<DomainReturnType>()
            .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted, ct);

        if (existing is null)
            throw new KeyNotFoundException("Return Type not found.");

        // Code is immutable per spec — do NOT update it
        existing.Description = entity.Description.Trim();
        existing.InventoryImpactId = entity.InventoryImpactId;
        existing.FinanceImpactId = entity.FinanceImpactId;
        existing.IsReplacementApplicable = entity.IsReplacementApplicable;
        existing.IsQcMandatory = entity.IsQcMandatory;
        existing.ApprovalRoleCode = entity.ApprovalRoleCode;
        existing.IsActive = entity.IsActive;

        await _db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
    {
        var existing = await _db.Set<DomainReturnType>()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

        if (existing is null)
            return false;

        existing.IsDeleted = IsDelete.Deleted;
        existing.IsActive = Status.Inactive;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
