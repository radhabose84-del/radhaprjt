using FAM.Application.Common.Interfaces.IUOM;
using FAM.Domain.Entities;
using FAM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Repositories.UOMs
{
    public class UOMCommandRepository : IUOMCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public UOMCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<(bool IsNameDuplicate, bool IsSortOrderDuplicate)> CheckForDuplicatesAsync(string name, int sortOrder, int excludeId)
        {
            var isNameDuplicate = await _applicationDbContext.UOMs
            .AnyAsync(ag => ag.UOMName == name && ag.Id != excludeId);

            var isSortOrderDuplicate = await _applicationDbContext.UOMs
            .AnyAsync(ag => ag.SortOrder == sortOrder && ag.Id != excludeId);

            return (isNameDuplicate, isSortOrderDuplicate);
        }

        public async Task<FAM.Domain.Entities.UOM> CreateAsync(FAM.Domain.Entities.UOM uom)
        {
            // Auto-generate SortOrder
            uom.SortOrder = await GetMaxSortOrderAsync() + 1;
            await _applicationDbContext.UOMs.AddAsync(uom);
            await _applicationDbContext.SaveChangesAsync();
            return uom;
        }

        public async Task<bool> DeleteAsync(int id, UOM uom)
        {
            var existingUom = await _applicationDbContext.UOMs.FirstOrDefaultAsync(u => u.Id == id);
            if (existingUom != null)
            {
                existingUom.IsDeleted = uom.IsDeleted;
                return await _applicationDbContext.SaveChangesAsync() >0;
            }
            return false; 
        }

        public async Task<int> GetMaxSortOrderAsync()
        {
            return await _applicationDbContext.UOMs.MaxAsync(ac => (int?)ac.SortOrder) ?? -1;

        }

        public async Task<bool> UpdateAsync(UOM uom)
        {
            var existingUom = await _applicationDbContext.UOMs.FirstOrDefaultAsync(u => u.Id == uom.Id);
            if (existingUom != null)
            {
                existingUom.Code = uom.Code;
                existingUom.UOMName = uom.UOMName;
                existingUom.SortOrder = uom.SortOrder;
                existingUom.UOMTypeId = uom.UOMTypeId;
                existingUom.IsActive = uom.IsActive;

                _applicationDbContext.UOMs.Update(existingUom);
                return await _applicationDbContext.SaveChangesAsync() > 0;
            }
            return false;
        }
    }
}