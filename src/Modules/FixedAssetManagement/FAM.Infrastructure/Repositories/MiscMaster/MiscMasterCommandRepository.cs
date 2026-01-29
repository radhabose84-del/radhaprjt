using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.Interfaces.IMiscMaster;
using FAM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Repositories.MiscMaster
{
  public class MiscMasterCommandRepository : IMiscMasterCommandRepository
  {
    private readonly ApplicationDbContext _dbContext;


    public MiscMasterCommandRepository(ApplicationDbContext applicationDbContext)
    {
      _dbContext = applicationDbContext;
    }

    public async Task<FAM.Domain.Entities.MiscMaster> CreateAsync(FAM.Domain.Entities.MiscMaster miscMaster)
    {

      // Auto-generate SortOrder
      miscMaster.SortOrder = await GetMaxSortOrderAsync() + 1;
      await _dbContext.MiscMaster.AddAsync(miscMaster);
      await _dbContext.SaveChangesAsync();
      return miscMaster;
    }
    public async Task<int> GetMaxSortOrderAsync()
    {
      return await _dbContext.MiscMaster.MaxAsync(ac => (int?)ac.SortOrder) ?? -1;
    }

    public async Task<bool> UpdateAsync(int id, FAM.Domain.Entities.MiscMaster miscMaster)
    {
      var existingMiscMaster = await _dbContext.MiscMaster.FirstOrDefaultAsync(m => m.Id == miscMaster.Id);

      if (existingMiscMaster != null)
      {
        existingMiscMaster.Code = miscMaster.Code;
        existingMiscMaster.MiscTypeId = miscMaster.MiscTypeId;
        existingMiscMaster.Description = miscMaster.Description;
        existingMiscMaster.SortOrder = miscMaster.SortOrder;
        existingMiscMaster.IsActive = miscMaster.IsActive;

        _dbContext.MiscMaster.Update(existingMiscMaster);
        return await _dbContext.SaveChangesAsync() > 0;
      }
      return false;
    }

    public async Task<bool> DeleteAsync(int id, FAM.Domain.Entities.MiscMaster miscMaster)
    {
      var existingMiscmaster = await _dbContext.MiscMaster.FirstOrDefaultAsync(u => u.Id == id);
      if (existingMiscmaster != null)
      {
        existingMiscmaster.IsDeleted = miscMaster.IsDeleted;
        return await _dbContext.SaveChangesAsync() > 0;
      }
      return false;
    }

    /// <summary>
    /// Save changes         03-07-2025
    /// </summary>
    /// <returns></returns>
    public async Task SaveChangesAsync()
    {
      await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(FAM.Domain.Entities.MiscMaster entity)
    {
      _dbContext.MiscMaster.Update(entity);
      await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> AlreadyExistsAsync(string code, int miscTypeId, int? id = null)
    {
      return await _dbContext.MiscMaster
          .AnyAsync(m => m.Code == code && m.MiscTypeId == miscTypeId && (id == null || m.Id != id));
    }
    public async Task AddAsync(FAM.Domain.Entities.MiscMaster entity)
    {
      await _dbContext.MiscMaster.AddAsync(entity);
    }
        public async Task<bool> UpdateMiscUploadAsync( FAM.Domain.Entities.MiscMaster miscMaster)
    {
        _dbContext.MiscMaster.Update(miscMaster);
        var result = await _dbContext.SaveChangesAsync();
        return result > 0;  // Return true if any rows affected
    }

    }
}