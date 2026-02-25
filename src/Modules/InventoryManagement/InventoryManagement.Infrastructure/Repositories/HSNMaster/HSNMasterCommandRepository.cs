using InventoryManagement.Application.Common.Interfaces.IHSNMaster;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Infrastructure.Repositories.HSNMaster
{
    public class HSNMasterCommandRepository : IHSNMasterCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public HSNMasterCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(InventoryManagement.Domain.Entities.HSNMaster hsnMaster)
        {
            await _applicationDbContext.HSNMaster.AddAsync(hsnMaster);
            await _applicationDbContext.SaveChangesAsync();
            return hsnMaster.Id;
        }


        public async Task<int> UpdateAsync(InventoryManagement.Domain.Entities.HSNMaster hsnMaster)
        {
            var existingEntity = await _applicationDbContext.HSNMaster
                .FirstOrDefaultAsync(x => x.Id == hsnMaster.Id && x.IsDeleted == InventoryManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0; // Not found


            existingEntity.TypeId = hsnMaster.TypeId;
            existingEntity.HSNCode = hsnMaster.HSNCode;
            existingEntity.Description = hsnMaster.Description;
            existingEntity.GSTCategoryId = hsnMaster.GSTCategoryId;
            existingEntity.GSTPercentage = hsnMaster.GSTPercentage;
            existingEntity.IGSTPercentage = hsnMaster.IGSTPercentage;
            existingEntity.ValidFrom = hsnMaster.ValidFrom;
            existingEntity.IsActive = hsnMaster.IsActive;

            existingEntity.ModifiedBy = hsnMaster.ModifiedBy;
            existingEntity.ModifiedByName = hsnMaster.ModifiedByName;
            existingEntity.ModifiedIP = hsnMaster.ModifiedIP;
            existingEntity.ModifiedDate = System.DateTimeOffset.UtcNow;

            _applicationDbContext.HSNMaster.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();

            return existingEntity.Id;
        }
        public async Task<bool> DeleteAsync(int id, InventoryManagement.Domain.Entities.HSNMaster hsnMaster)
        {
            var existing = await _applicationDbContext.HSNMaster
                .FirstOrDefaultAsync(x => x.Id == id );

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;            

            _applicationDbContext.HSNMaster.Update(existing);
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }



    }
}