using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IBillEntry;
using PurchaseManagement.Domain.Entities.PurchaseOrder.BillEntry;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.BillEntry;

public class PurchaseBillEntryCommandRepository : IPurchaseBillEntryCommandRepository
{
    private readonly ApplicationDbContext _db;

    public PurchaseBillEntryCommandRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(PurchaseBillEntryHeader entity, CancellationToken ct = default)
    {
        await _db.Set<PurchaseBillEntryHeader>().AddAsync(entity, ct);
        await _db.SaveChangesAsync(ct);
    }
    public async Task UpdateAsync(PurchaseBillEntryHeader entity, CancellationToken ct = default)
    {
        _db.Set<PurchaseBillEntryHeader>().Update(entity);
        await _db.SaveChangesAsync(ct);
    }
    public async Task DeleteByGrnIdAsync(int grnId, CancellationToken ct = default)
    {
        var headers = await _db.Set<PurchaseBillEntryHeader>()
            .Include(h => h.Lines)
            .Where(h => h.GrnId == grnId 
                        && h.IsActive == Status.Active 
                        && h.IsDeleted == IsDelete.NotDeleted)
            .ToListAsync(ct);

        if (!headers.Any())
            return;

        _db.RemoveRange(headers.SelectMany(h => h.Lines));
        _db.RemoveRange(headers);

        await _db.SaveChangesAsync(ct);
    }
}
