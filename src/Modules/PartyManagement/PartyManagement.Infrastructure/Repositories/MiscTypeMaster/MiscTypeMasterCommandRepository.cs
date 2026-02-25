using PartyManagement.Application.Common.Interfaces.IMiscTypeMaster;
using Microsoft.EntityFrameworkCore;
using PartyManagement.Infrastructure.Data;

namespace PartyManagement.Infrastructure.Repositories.MiscTypeMaster
{
    public class MiscTypeMasterCommandRepository : IMiscTypeMasterCommandRepository
    {

        private readonly ApplicationDbContext _dbContext;


        public MiscTypeMasterCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _dbContext = applicationDbContext;
        }
         public async   Task<PartyManagement.Domain.Entities.MiscTypeMaster> CreateAsync(PartyManagement.Domain.Entities.MiscTypeMaster miscTypeMaster)
        {
             await _dbContext.MiscTypeMaster.AddAsync(miscTypeMaster);
                await _dbContext.SaveChangesAsync();
                return miscTypeMaster;
        }


          public async Task<bool> UpdateAsync(int id,PartyManagement.Domain.Entities.MiscTypeMaster miscTypeMaster)
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
        public async Task<bool> DeleteAsync(int id,PartyManagement.Domain.Entities.MiscTypeMaster miscTypeMaster)
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