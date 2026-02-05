using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemLogById;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemLogs;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Queries
{
    public sealed class ItemLogQueryRepository : IItemLogQueryRepository
    {
        private readonly ApplicationDbContext _db;
        public ItemLogQueryRepository(ApplicationDbContext db) => _db = db;

        public async Task<ItemLogDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.ItemLog.AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new ItemLogDto
                {
                    Id = x.Id,
                    EntityName = x.EntityName,
                    EntityId = x.EntityId,
                    Action = x.Action,
                    PropertyName = x.PropertyName,
                    OldValue = x.OldValue,
                    NewValue = x.NewValue,
                    CreatedBy = x.CreatedBy,
                    CreatedByName = x.CreatedByName,
                    CreatedIP = x.CreatedIP,
                    CreatedDate = x.CreatedDate
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<(List<ItemLogDto> Items, int TotalCount)> GetAllAsync(
            ItemLogFilter f, CancellationToken ct = default)
        {
            var q = _db.ItemLog.AsNoTracking().AsQueryable();

            if (f.EntityId.HasValue && f.EntityId > 0)
                q = q.Where(x => x.EntityId == f.EntityId.Value);
            
            if (f.UserId.HasValue && f.UserId > 0)
                q = q.Where(x => x.CreatedBy == f.UserId.Value);

            if (f.From.HasValue)
                q = q.Where(x => x.CreatedDate >= f.From.Value);

            if (f.To.HasValue)
                q = q.Where(x => x.CreatedDate <= f.To.Value);

            if (!string.IsNullOrWhiteSpace(f.Search))
            {
                var term = f.Search.Trim();
                q = q.Where(x =>
                    x.PropertyName.Contains(term) ||
                    (x.OldValue != null && x.OldValue.Contains(term)) ||
                    (x.NewValue != null && x.NewValue.Contains(term)));
            }

            var total = await q.CountAsync(ct);

            // newest first
            q = q.OrderByDescending(x => x.CreatedDate).ThenByDescending(x => x.Id);

            var projected = q.Select(x => new ItemLogDto
            {
                Id = x.Id,
                EntityName = x.EntityName,
                EntityId = x.EntityId,
                Action = x.Action,
                PropertyName = x.PropertyName,
                OldValue = x.OldValue,
                NewValue = x.NewValue,
                CreatedBy = x.CreatedBy,
                CreatedByName = x.CreatedByName,
                CreatedIP = x.CreatedIP,
                CreatedDate = x.CreatedDate
            });

            List<ItemLogDto> items;
            bool paginate = f.Page.HasValue && f.Size.HasValue && f.Page > 0 && f.Size > 0;
            if (paginate)
            {
                var skip = (f.Page!.Value - 1) * f.Size!.Value;
                items = await projected.Skip(skip).Take(f.Size!.Value).ToListAsync(ct);
            }
            else
            {
                items = await projected.ToListAsync(ct);
            }

            return (items, total);
        }

        public Task<(List<ItemLogDto> Items, int TotalCount)> GetForEntityAsync(
            string entityName, int entityId, int? page, int? size, CancellationToken ct = default)
        {
            var filter = new ItemLogFilter
            {                
                EntityId = entityId,
                Page = page,
                Size = size
            };
            return GetAllAsync(filter, ct);
        }
    }
}
