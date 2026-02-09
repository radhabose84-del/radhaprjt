using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.Interfaces.IMiscTypeMaster;
using Dapper;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Infrastructure.Data;

namespace PurchaseManagement.Infrastructure.Repositories.MiscTypeMaster
{
    public class MiscTypeMasterCommandRepository  : IMiscTypeMasterCommandRepository
    {
        
       private readonly ApplicationDbContext _dbContext;      

          public MiscTypeMasterCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _dbContext = applicationDbContext;
        }                 
     public async   Task<Core.Domain.Entities.MiscTypeMaster> CreateAsync(Core.Domain.Entities.MiscTypeMaster miscTypeMaster)
        {
            await _dbContext.MiscTypeMaster.AddAsync(miscTypeMaster);
            await _dbContext.SaveChangesAsync();
            return miscTypeMaster;
        }


          public async Task<bool> UpdateAsync(int id,Core.Domain.Entities.MiscTypeMaster miscTypeMaster)
        {
            var existingMiscTypeMaster =await _dbContext.MiscTypeMaster.FirstOrDefaultAsync(m =>m.Id == miscTypeMaster.Id);
         
            if (existingMiscTypeMaster != null)
            {
                existingMiscTypeMaster.MiscTypeCode = miscTypeMaster.MiscTypeCode;
                existingMiscTypeMaster.Description = miscTypeMaster.Description;               
                existingMiscTypeMaster.IsActive = miscTypeMaster.IsActive;

                _dbContext.MiscTypeMaster.Update(existingMiscTypeMaster);
                return await _dbContext.SaveChangesAsync() > 0;
            }
            return false;
        }
        public async Task<bool> DeleteAsync(int id,Core.Domain.Entities.MiscTypeMaster miscTypeMaster)
        {
            var existingMiscTypemaster = await _dbContext.MiscTypeMaster.FirstOrDefaultAsync(u => u.Id == id);
            if (existingMiscTypemaster != null)
            {
                existingMiscTypemaster.IsDeleted = miscTypeMaster.IsDeleted;
                return await _dbContext.SaveChangesAsync() >0;
            }
            return false; 
        }        
       

    }
   
}