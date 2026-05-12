using PurchaseManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;

public sealed class ActivityLogQueryRepository : IActivityLogQueryRepository
{
    private readonly ApplicationDbContext _db;
    public ActivityLogQueryRepository(ApplicationDbContext db) => _db = db;

    public async Task<(List<ActivityLog>, int)> GetAllAsync(string entityName, int entityId, int page, int size, CancellationToken ct)
    {
        var q = _db.Set<ActivityLog>()
                   .AsNoTracking()
                   .Where(x => x.EntityName == entityName && x.EntityId == entityId);

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(x => x.CreatedDate)
                           .Skip((page - 1) * size)
                           .Take(size)
                           .ToListAsync(ct);
        return (items, total);
    }

    public Task<ActivityLog?> GetByIdAsync(long id, CancellationToken ct)
        => _db.Set<ActivityLog>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<(List<ActivityLog>, int)> GetByQuotationIdAsync(int quotationHeaderId, int page, int size, CancellationToken ct)
    {
        // Get all detail line IDs belonging to this header
        var detailIds = await _db.QuotationDetails
            .AsNoTracking()
            .Where(d => d.QuotationHeaderId == quotationHeaderId)
            .Select(d => d.Id)
            .ToListAsync(ct);

        // Query logs for header + all its detail lines
        var q = _db.Set<ActivityLog>()
                   .AsNoTracking()
                   .Where(x =>
                       (x.EntityName == "QuotationHeader" && x.EntityId == quotationHeaderId) ||
                       (x.EntityName == "QuotationDetail" && detailIds.Contains(x.EntityId)));

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(x => x.CreatedDate)
                           .Skip((page - 1) * size)
                           .Take(size)
                           .ToListAsync(ct);
        return (items, total);
    }
}
