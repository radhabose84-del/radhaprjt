
using InventoryManagement.Application.Common.Interfaces.Item.ItemCategory;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories.Item.ItemCategory
{
    public class ItemCategoryCommandRepository : IItemCategoryCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public ItemCategoryCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(InventoryManagement.Domain.Entities.Item.ItemCategory itemCategory)
        {                
            await _applicationDbContext.ItemCategory.AddAsync(itemCategory);
            await _applicationDbContext.SaveChangesAsync();     
             // Step 2: Determine RootCategoryId
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

            // Step 3: Update the RootCategoryId
            _applicationDbContext.ItemCategory.Update(itemCategory);
            await _applicationDbContext.SaveChangesAsync();           
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
            await _applicationDbContext.SaveChangesAsync();
            return 1; 
        }
        public async Task<int> UpdateAsync(int Id, InventoryManagement.Domain.Entities.Item.ItemCategory itemCategory)
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
            existingItemCategory.IsActive=itemCategory.IsActive;
            
            _applicationDbContext.ItemCategory.Update(existingItemCategory);
            
            await _applicationDbContext.SaveChangesAsync();
            return 1;
        }
        public async Task<bool> ExistsByNameAsync(string? name)
        {
            return await _applicationDbContext.ItemCategory
                .Where(cc => cc.ItemCategoryName == name  && cc.IsDeleted == 0)
                .AnyAsync();
        }

        public async Task<bool> IsNameDuplicateAsync(string? name, int excludeId)
        {
            return await _applicationDbContext.ItemCategory
                .Where(cc => cc.ItemCategoryName == name  && cc.Id != excludeId && cc.IsDeleted == 0)
                .AnyAsync();
        }   
    }
}