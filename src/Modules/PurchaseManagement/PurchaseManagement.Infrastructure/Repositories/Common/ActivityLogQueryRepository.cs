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
}
