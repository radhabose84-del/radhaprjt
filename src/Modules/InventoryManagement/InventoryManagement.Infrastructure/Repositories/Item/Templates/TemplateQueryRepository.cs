using InventoryManagement.Application.Common.Interfaces.Item.Templates;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Templates;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Data.Repositories.Item.Templates
{
    internal sealed class TemplateQueryRepository : ITemplateQueryRepository
    {
        private readonly ApplicationDbContext _db;
        public TemplateQueryRepository(ApplicationDbContext db) => _db = db;

        public async Task<InspectionTemplate?> GetByIdAsync(int id, CancellationToken ct)
        {
            return await _db.InspectionTemplate
                .AsNoTracking()
                .Include(t => t.Parameters)
                .FirstOrDefaultAsync(t => t.Id == id && t.IsDeleted == InventoryManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted, ct);
        }

        public async Task<(IReadOnlyList<InspectionTemplate> Items, int TotalCount)> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct)
        {
            var q = _db.InspectionTemplate
                .AsNoTracking()
                .Include(t => t.Parameters)
                .Where(t => t.IsDeleted == InventoryManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                q = q.Where(t => t.TemplateName.ToLower().Contains(term));
            }

            var total = await q.CountAsync(ct);

            var items = await q
                .OrderByDescending(t => t.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<IReadOnlyList<InspectionTemplate>> GetAutoCompleteAsync(string? search, int take, CancellationToken ct)
        {
            var q = _db.InspectionTemplate
                .AsNoTracking()
                .Where(t => t.IsDeleted == InventoryManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                q = q.Where(t => t.TemplateName.ToLower().Contains(term));
            }

            return await q.OrderByDescending(t => t.Id)
                          .Take(take)
                          .ToListAsync(ct);
        }

        public async Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken ct)
        {
            var trimmed = name.Trim().ToLower();
            var q = _db.InspectionTemplate.AsNoTracking()
                .Where(t => t.IsDeleted == InventoryManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted)
                .Where(t => t.TemplateName.ToLower() == trimmed);

            if (excludeId.HasValue)
                q = q.Where(t => t.Id != excludeId.Value);

            return await q.AnyAsync(ct);
        }
        public async Task<bool> ExistsByIdAsync(int id, CancellationToken ct)
        {
            return await _db.InspectionTemplate
                .AsNoTracking()
                .AnyAsync(t => t.Id == id && t.IsDeleted == BaseEntity.IsDelete.NotDeleted, ct);
        }

        public async Task<bool> SoftDeleteValidationAsync(int id, CancellationToken ct)
        {
            // Check InspectionParameter (has IsDeleted)
            var hasParams = await _db.InspectionParameter
                .AsNoTracking()
                .AnyAsync(p => p.TemplateId == id && p.IsDeleted == BaseEntity.IsDelete.NotDeleted, ct);
            if (hasParams) return true;

            // Check ItemQuality (no IsDeleted — any row = linked)
            var hasItems = await _db.Database
                .SqlQueryRaw<int>(
                    "SELECT CASE WHEN EXISTS (SELECT 1 FROM [Inventory].[ItemQuality] WHERE InspectionTemplateId = {0}) THEN 1 ELSE 0 END AS [Value]",
                    id)
                .FirstOrDefaultAsync(ct);
            return hasItems == 1;
        }

        public async Task<bool> IsTemplateLinkedAsync(int id, CancellationToken ct)
        {
            // Check InspectionParameter (has IsDeleted + IsActive)
            var hasActiveParams = await _db.InspectionParameter
                .AsNoTracking()
                .AnyAsync(p => p.TemplateId == id
                    && p.IsDeleted == BaseEntity.IsDelete.NotDeleted
                    && p.IsActive == BaseEntity.Status.Active, ct);
            if (hasActiveParams) return true;

            // Check ItemQuality (no IsDeleted/IsActive — any row = linked)
            var hasItems = await _db.Database
                .SqlQueryRaw<int>(
                    "SELECT CASE WHEN EXISTS (SELECT 1 FROM [Inventory].[ItemQuality] WHERE InspectionTemplateId = {0}) THEN 1 ELSE 0 END AS [Value]",
                    id)
                .FirstOrDefaultAsync(ct);
            return hasItems == 1;
        }
    }
}
