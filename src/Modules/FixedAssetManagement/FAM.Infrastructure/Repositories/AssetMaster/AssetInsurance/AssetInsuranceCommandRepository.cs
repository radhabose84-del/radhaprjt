using System.Linq.Expressions;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetInsurance;
using FAM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace FAM.Infrastructure.Repositories.AssetMaster.AssetInsurance
{
    public class AssetInsuranceCommandRepository  : IAssetInsuranceCommandRepository
    {
          private readonly ApplicationDbContext _dbContext;

            public AssetInsuranceCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _dbContext = applicationDbContext;
        }

        public async  Task<FAM.Domain.Entities.AssetMaster.AssetInsurance> CreateAsync(FAM.Domain.Entities.AssetMaster.AssetInsurance assetInsurance)
        {            


             await _dbContext.AssetInsurance.AddAsync(assetInsurance);
                await _dbContext.SaveChangesAsync();
                return assetInsurance;
        }  
        
           public async Task<bool> UpdateAsync(int id,FAM.Domain.Entities.AssetMaster.AssetInsurance assetInsurance)
        {
            var existingassetInsurance =await _dbContext.AssetInsurance.FirstOrDefaultAsync(m =>m.Id == id);
         
            if (existingassetInsurance != null)
            {
                existingassetInsurance.PolicyNo = assetInsurance.PolicyNo;
                existingassetInsurance.StartDate = assetInsurance.StartDate;   
                existingassetInsurance.Insuranceperiod = assetInsurance.Insuranceperiod;            
                existingassetInsurance.EndDate = assetInsurance.EndDate;
                existingassetInsurance.PolicyAmount = assetInsurance.PolicyAmount;
                existingassetInsurance.VendorCode = assetInsurance.VendorCode;               
                existingassetInsurance.RenewalStatus = assetInsurance.RenewalStatus;
                existingassetInsurance.RenewedDate = assetInsurance.RenewedDate;
                existingassetInsurance.IsActive = assetInsurance.IsActive;               
               

                _dbContext.AssetInsurance.Update(existingassetInsurance);
                return await _dbContext.SaveChangesAsync() > 0;
            }
            return false;
        }


        public async Task<bool> DeleteAsync(int id,FAM.Domain.Entities.AssetMaster.AssetInsurance assetInsurance)
        {
        
             var existingAssetInsurance = await _dbContext.AssetInsurance.FirstOrDefaultAsync(u => u.Id == id);
            if (existingAssetInsurance != null)
            {
                existingAssetInsurance.IsDeleted = assetInsurance.IsDeleted;
                return await _dbContext.SaveChangesAsync() >0;
            }
            return false; 
        }
           public async Task<FAM.Domain.Entities.AssetMaster.AssetInsurance?> GetAlreadyAsync(Expression<Func<FAM.Domain.Entities.AssetMaster.AssetInsurance, bool>> predicate)
        {
         

            return await _dbContext.AssetInsurance.FirstOrDefaultAsync(predicate);
        }


        
    }
}