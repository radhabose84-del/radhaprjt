using InventoryManagement.Application.Common.Interfaces.Item.ItemCategory;
using InventoryManagement.Domain.Entities.Item;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Infrastructure.Repositories.Item.ItemCategory
{
    public class ItemCategoryCommandRepository : IItemCategoryCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public ItemCategoryCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(
            InventoryManagement.Domain.Entities.Item.ItemCategory itemCategory,
            List<int> moduleIds,
            List<ItemCategoryUnitConfig> unitConfigs)
        {
            await _applicationDbContext.ItemCategory.AddAsync(itemCategory);
            await _applicationDbContext.SaveChangesAsync();

            if (itemCategory.ParentCategoryId == null)
            {
                itemCategory.RootCategoryId = itemCategory.Id;
            }
            else
            {
                var parent = await _applicationDbContext.ItemCategory
                    .Where(p => p.Id == itemCategory.ParentCategoryId)
                    .Select(p => new { p.RootCategoryId })
                    .FirstOrDefaultAsync();

                itemCategory.RootCategoryId = parent?.RootCategoryId ?? itemCategory.ParentCategoryId;
            }

            _applicationDbContext.ItemCategory.Update(itemCategory);
            await _applicationDbContext.SaveChangesAsync();

            var distinctModuleIds = moduleIds?.Distinct().ToList() ?? new List<int>();
            if (distinctModuleIds.Count > 0)
            {
                var rows = distinctModuleIds.Select(mid => new ItemCategoryModule
                {
                    ItemCategoryId = itemCategory.Id,
                    ModuleId = mid
                }).ToList();
                await _applicationDbContext.ItemCategoryModule.AddRangeAsync(rows);
                await _applicationDbContext.SaveChangesAsync();
            }

            var newConfigs = (unitConfigs ?? new List<ItemCategoryUnitConfig>())
                .GroupBy(c => c.UnitId)
                .Select(g => g.First())
                .ToList();
            if (newConfigs.Count > 0)
            {
                foreach (var c in newConfigs)
                {
                    c.Id = 0;
                    c.ItemCategoryId = itemCategory.Id;
                    c.IsDeleted = IsDelete.NotDeleted;
                }
                await _applicationDbContext.ItemCategoryUnitConfig.AddRangeAsync(newConfigs);
                await _applicationDbContext.SaveChangesAsync();
            }

            return itemCategory.Id;
        }

        public async Task<int> DeleteAsync(int Id, InventoryManagement.Domain.Entities.Item.ItemCategory itemCategory)
        {
            var itemCategoryToDelete = await _applicationDbContext.ItemCategory.FirstOrDefaultAsync(u => u.Id == Id);
            if (itemCategoryToDelete is null)
            {
                return -1;
            }
            itemCategoryToDelete.IsDeleted = itemCategory.IsDeleted;

            var liveChildren = await _applicationDbContext.ItemCategoryUnitConfig
                .Where(c => c.ItemCategoryId == Id && c.IsDeleted == IsDelete.NotDeleted)
                .ToListAsync();
            foreach (var child in liveChildren)
            {
                child.IsDeleted = IsDelete.Deleted;
            }

            await _applicationDbContext.SaveChangesAsync();
            return 1;
        }

        public async Task<int> UpdateAsync(
            int Id,
            InventoryManagement.Domain.Entities.Item.ItemCategory itemCategory,
            List<int> moduleIds,
            List<ItemCategoryUnitConfig> unitConfigs)
        {
            var existingItemCategory = await _applicationDbContext.ItemCategory.FirstOrDefaultAsync(u => u.Id == Id);
            if (existingItemCategory is null)
            {
                return -1;
            }
            existingItemCategory.ItemCategoryName = itemCategory.ItemCategoryName;
            existingItemCategory.ItemGroupId = itemCategory.ItemGroupId;
            existingItemCategory.IsGroup = itemCategory.IsGroup;
            existingItemCategory.ParentCategoryId = itemCategory.ParentCategoryId;
            existingItemCategory.IsBudgetApplicable = itemCategory.IsBudgetApplicable;
            existingItemCategory.IsActive = itemCategory.IsActive;
			existingItemCategory.IsActive=itemCategory.IsActive;

            _applicationDbContext.ItemCategory.Update(existingItemCategory);
            await _applicationDbContext.SaveChangesAsync();

            var existingModuleRows = await _applicationDbContext.ItemCategoryModule
                .Where(m => m.ItemCategoryId == Id)
                .ToListAsync();
            if (existingModuleRows.Count > 0)
            {
                _applicationDbContext.ItemCategoryModule.RemoveRange(existingModuleRows);
                await _applicationDbContext.SaveChangesAsync();
            }

            var distinctModuleIds = moduleIds?.Distinct().ToList() ?? new List<int>();
            if (distinctModuleIds.Count > 0)
            {
                var rows = distinctModuleIds.Select(mid => new ItemCategoryModule
                {
                    ItemCategoryId = Id,
                    ModuleId = mid
                }).ToList();
                await _applicationDbContext.ItemCategoryModule.AddRangeAsync(rows);
                await _applicationDbContext.SaveChangesAsync();
            }

            var payload = (unitConfigs ?? new List<ItemCategoryUnitConfig>())
                .GroupBy(c => c.UnitId)
                .Select(g => g.First())
                .ToList();

            var existingConfigs = await _applicationDbContext.ItemCategoryUnitConfig
                .Where(c => c.ItemCategoryId == Id)
                .ToListAsync();

            var payloadIds = payload.Where(p => p.Id > 0).Select(p => p.Id).ToHashSet();
            var hasSoftDeletes = false;
            foreach (var existing in existingConfigs)
            {
                if (!payloadIds.Contains(existing.Id) && existing.IsDeleted == IsDelete.NotDeleted)
                {
                    existing.IsDeleted = IsDelete.Deleted;
                    hasSoftDeletes = true;
                }
            }

            if (hasSoftDeletes)
            {
                await _applicationDbContext.SaveChangesAsync();
            }

            var existingById = existingConfigs.ToDictionary(e => e.Id);
            foreach (var p in payload)
            {
                if (p.Id > 0 && existingById.TryGetValue(p.Id, out var existing))
                {
                    existing.UnitId = p.UnitId;
                    existing.UOMId = p.UOMId;
                    existing.MaxSampleQuantity = p.MaxSampleQuantity;
                    existing.IsActive = p.IsActive;
                    existing.IsDeleted = IsDelete.NotDeleted;
                }
            }

            var newRows = payload
                .Where(p => p.Id == 0)
                .Select(p => new ItemCategoryUnitConfig
                {
                    ItemCategoryId = Id,
                    UnitId = p.UnitId,
                    UOMId = p.UOMId,
                    MaxSampleQuantity = p.MaxSampleQuantity,
                    IsActive = p.IsActive,
                    IsDeleted = IsDelete.NotDeleted
                })
                .ToList();

            if (newRows.Count > 0)
            {
                await _applicationDbContext.ItemCategoryUnitConfig.AddRangeAsync(newRows);
            }

            await _applicationDbContext.SaveChangesAsync();

            return 1;
        }

        public async Task<bool> ExistsByNameAsync(string? name)
        {
            return await _applicationDbContext.ItemCategory
                .Where(cc => cc.ItemCategoryName == name && cc.IsDeleted == 0)
                .AnyAsync();
        }

        public async Task<bool> IsNameDuplicateAsync(string? name, int excludeId)
        {
            return await _applicationDbContext.ItemCategory
                .Where(cc => cc.ItemCategoryName == name && cc.Id != excludeId && cc.IsDeleted == 0)
                .AnyAsync();
        }
    }
}
