using UserManagement.Application.Common.Interfaces.IMiscTypeMaster;
using Microsoft.EntityFrameworkCore;
using UserManagement.Infrastructure.Data;

namespace UserManagement.Infrastructure.Repositories.MiscTypeMaster
{
    public class MiscTypeMasterCommandRepository: IMiscTypeMasterCommandRepository
    {
        
       private readonly ApplicationDbContext _dbContext;
      

          public MiscTypeMasterCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _dbContext = applicationDbContext;
        }                 
     public async   Task<UserManagement.Domain.Entities.MiscTypeMaster> CreateAsync(UserManagement.Domain.Entities.MiscTypeMaster miscTypeMaster)
        {
             await _dbContext.MiscTypeMaster.AddAsync(miscTypeMaster);
                await _dbContext.SaveChangesAsync();
                return miscTypeMaster;
        }


          public async Task<bool> UpdateAsync(int id, UserManagement.Domain.Entities.MiscTypeMaster miscTypeMaster)
        {
            var stub = new UserManagement.Domain.Entities.MiscTypeMaster { Id = miscTypeMaster.Id };
            _dbContext.Attach(stub);
            stub.MiscTypeCode = miscTypeMaster.MiscTypeCode;
            stub.Description = miscTypeMaster.Description;
            stub.IsActive = miscTypeMaster.IsActive;
            _dbContext.Entry(stub).Property(x => x.MiscTypeCode).IsModified = true;
            _dbContext.Entry(stub).Property(x => x.Description).IsModified = true;
            _dbContext.Entry(stub).Property(x => x.IsActive).IsModified = true;
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id, UserManagement.Domain.Entities.MiscTypeMaster miscTypeMaster)
        {
            var stub = new UserManagement.Domain.Entities.MiscTypeMaster { Id = id };
            _dbContext.Attach(stub);
            stub.IsDeleted = miscTypeMaster.IsDeleted;
            _dbContext.Entry(stub).Property(x => x.IsDeleted).IsModified = true;
            return await _dbContext.SaveChangesAsync() > 0;
        }        
               
    }
}