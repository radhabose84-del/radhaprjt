using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeeder;
using MaintenanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceManagement.Infrastructure.Repositories.Power.Feeder
{
    public class FeederCommandRepository : IFeederCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public FeederCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _dbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(MaintenanceManagement.Domain.Entities.Power.Feeder feeder)
        {
            await _dbContext.Feeder.AddAsync(feeder);
            await _dbContext.SaveChangesAsync();
            return feeder.Id;
        }
        
        public async Task<bool> UpdateAsync(int id, MaintenanceManagement.Domain.Entities.Power.Feeder feeder)
        {
           var existingFeeder = await _dbContext.Feeder.FirstOrDefaultAsync(u => u.Id == id);
            if (existingFeeder != null)
            {
                existingFeeder.FeederCode = feeder.FeederCode;
                existingFeeder.FeederName = feeder.FeederName;
                existingFeeder.UnitId = feeder.UnitId;
                existingFeeder.ParentFeederId = feeder.ParentFeederId;
                existingFeeder.FeederGroupId = feeder.FeederGroupId;
                existingFeeder.FeederTypeId = feeder.FeederTypeId;
                existingFeeder.DepartmentId = feeder.DepartmentId;
                existingFeeder.Description = feeder.Description;
                existingFeeder.MultiplicationFactor = feeder.MultiplicationFactor;
                existingFeeder.EffectiveDate = feeder.EffectiveDate;
                existingFeeder.OpeningReading = feeder.OpeningReading;
                existingFeeder.MeterAvailable = feeder.MeterAvailable;
                existingFeeder.MeterTypeId = feeder.MeterTypeId;
                existingFeeder.HighPriority = feeder.HighPriority; 
                existingFeeder.Target = feeder.Target;                 
                existingFeeder.IsActive = feeder.IsActive;
                _dbContext.Feeder.Update(existingFeeder);
                return await _dbContext.SaveChangesAsync() > 0;
            }
            return false;
        }
           public async Task<bool> DeleteAsync(int id, MaintenanceManagement.Domain.Entities.Power.Feeder feeder)
        {
            var existingfeeder = await _dbContext.Feeder.FirstOrDefaultAsync(u => u.Id == id);
            if (existingfeeder != null)
            {
                existingfeeder.IsDeleted = feeder.IsDeleted;
                return await _dbContext.SaveChangesAsync() >0;
            }
            return false; 
        }        
    }
}