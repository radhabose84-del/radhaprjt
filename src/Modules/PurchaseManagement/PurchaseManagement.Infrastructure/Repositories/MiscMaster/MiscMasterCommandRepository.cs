using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;

namespace PurchaseManagement.Infrastructure.Repositories.MiscMaster
{
    public class MiscMasterCommandRepository : IMiscMasterCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;


        public MiscMasterCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _dbContext = applicationDbContext;
        }

        public async Task<PurchaseManagement.Domain.Entities.MiscMaster> CreateAsync(PurchaseManagement.Domain.Entities.MiscMaster miscMaster)
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

        public async Task<bool> UpdateAsync(int id, PurchaseManagement.Domain.Entities.MiscMaster miscMaster)
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

        public async Task<bool> DeleteAsync(int id, PurchaseManagement.Domain.Entities.MiscMaster miscMaster)
        {
            var existingMiscmaster = await _dbContext.MiscMaster.FirstOrDefaultAsync(u => u.Id == id);
            if (existingMiscmaster != null)
            {
                existingMiscmaster.IsDeleted = miscMaster.IsDeleted;
                return await _dbContext.SaveChangesAsync() > 0;
            }
            return false;
        }   
         public async Task<Dictionary<int,PurchaseManagement.Domain.Entities.MiscMaster>> GetManyByIdsAsync(IEnumerable<int> ids, CancellationToken ct)
    {
        var idList = ids?.Distinct().ToList() ?? new List<int>();
        if (idList.Count == 0) return new Dictionary<int, PurchaseManagement.Domain.Entities.MiscMaster>();

        var rows = await _dbContext.MiscMaster
            .AsNoTracking()
            .Where(m => idList.Contains(m.Id))
            .ToListAsync(ct);

        return rows.ToDictionary(x => x.Id, x => x);
    }
        
    }
}