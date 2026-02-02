using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IBillEntry;
using PurchaseManagement.Domain.Entities.PurchaseOrder.BillEntry;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;

namespace PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.BillEntry;

public class PurchaseBillEntryQueryRepository : IPurchaseBillEntryQueryRepository
{
    private readonly ApplicationDbContext _db;

    public PurchaseBillEntryQueryRepository(ApplicationDbContext db)
    {
        _db = db;
    }
    public async Task<PurchaseBillEntryHeader?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Set<PurchaseBillEntryHeader>()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<(IReadOnlyList<PurchaseBillEntryHeader> Rows, int Total)>
    GetListAsync(
        int? partyId,
        string? search,
        DateOnly? fromDate,
        DateOnly? toDate,
        int page,
        int size,
        CancellationToken ct = default)
    {
        var query = _db.Set<PurchaseBillEntryHeader>()
            .Include(x => x.Lines)           // 👈 load line items
            .AsQueryable();

        if (partyId.HasValue && partyId.Value > 0)
            query = query.Where(x => x.PartyId == partyId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var like = search.Trim();
            query = query.Where(x => x.BillNumber.Contains(like));
        }

        if (fromDate.HasValue)
            query = query.Where(x => x.BillDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(x => x.BillDate <= toDate.Value);

        var total = await query.CountAsync(ct);

        var rows = await query
            .OrderByDescending(x => x.BillDate)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return (rows, total);
    }


    public async Task<IReadOnlyList<PurchaseBillEntryHeader>> GetAutoCompleteAsync(
        string? search,
        int take,
        CancellationToken ct = default)
    {
        var query = _db.Set<PurchaseBillEntryHeader>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var like = search.Trim();
            query = query.Where(x => x.BillNumber.Contains(like));
        }

        return await query
            .OrderByDescending(x => x.BillDate)
            .Take(take)
            .ToListAsync(ct);
    }

    public Task<bool> BillNumberExistsAsync(
        int partyId,
        string billNumber,
        int? excludeId,
        CancellationToken ct = default)
    {
        var q = _db.Set<PurchaseBillEntryHeader>()
            .Where(x => x.PartyId == partyId && x.BillNumber == billNumber);

        if (excludeId.HasValue)
            q = q.Where(x => x.Id != excludeId.Value);

        return q.AnyAsync(ct);
    }
}
