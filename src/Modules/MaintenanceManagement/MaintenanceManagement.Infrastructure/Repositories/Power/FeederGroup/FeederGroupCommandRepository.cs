using MaintenanceManagement.Application.Common.Interfaces.Power.IFeederGroup;
using MaintenanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceManagement.Infrastructure.Repositories.Power.FeederGroup
{
    public class FeederGroupCommandRepository : IFeederGroupCommandRepository
    {

        private readonly ApplicationDbContext _dbContext;

        public FeederGroupCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _dbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(MaintenanceManagement.Domain.Entities.Power.FeederGroup feederGroup)
        {
            await _dbContext.FeederGroup.AddAsync(feederGroup);
            await _dbContext.SaveChangesAsync();
            return feederGroup.Id;
        }

        public async Task<bool> UpdateAsync(int id, MaintenanceManagement.Domain.Entities.Power.FeederGroup feederGroup)
        {
            var existingFeederGroup = await _dbContext.FeederGroup.FirstOrDefaultAsync(u => u.Id == id);
            if (existingFeederGroup != null)
            {
                existingFeederGroup.FeederGroupCode = feederGroup.FeederGroupCode;
                existingFeederGroup.FeederGroupName = feederGroup.FeederGroupName;
                existingFeederGroup.UnitId = feederGroup.UnitId;
                existingFeederGroup.IsActive = feederGroup.IsActive;
                _dbContext.FeederGroup.Update(existingFeederGroup);
                return await _dbContext.SaveChangesAsync() > 0;
            }
            return false;

        }

          public async Task<bool> DeleteAsync(int id, MaintenanceManagement.Domain.Entities.Power.FeederGroup feederGroup)
        {
            var existingfeederGroup = await _dbContext.FeederGroup.FirstOrDefaultAsync(u => u.Id == id);
            if (existingfeederGroup != null)
            {
                existingfeederGroup.IsDeleted = feederGroup.IsDeleted;
                return await _dbContext.SaveChangesAsync() >0;
            }
            return false; 
        }        

        
    }
}