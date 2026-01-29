using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAmc;
using FAM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Repositories.AssetMaster.AssetAmc
{
    public class AssetAmcCommandRepository : IAssetAmcCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public AssetAmcCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(FAM.Domain.Entities.AssetMaster.AssetAmc assetAmc)
        {
            // Add the AssetAdditionalCost to the DbContext
        await _applicationDbContext.AssetAmc.AddAsync(assetAmc);

        // Save changes to the database
        await _applicationDbContext.SaveChangesAsync();

        // Return the ID of the created AssetAdditionalCost
        return assetAmc.Id;
        }

        public async Task<int> DeleteAsync(int Id, FAM.Domain.Entities.AssetMaster.AssetAmc assetAmc)
        {
            // Fetch the assetGroup to delete from the database
            var assetAmcToDelete = await _applicationDbContext.AssetAmc.FirstOrDefaultAsync(u => u.Id == Id);

            // If the assetGroup does not exist
            if (assetAmcToDelete is null)
            {
                return -1; //indicate failure
            }

            // Update the IsActive status to indicate deletion (or soft delete)
            assetAmcToDelete.IsDeleted = assetAmc.IsDeleted;

            // Save changes to the database 
            await _applicationDbContext.SaveChangesAsync();

            return 1; // Indicate success
        }

        public async Task<FAM.Domain.Entities.AssetMaster.AssetAmc?> GetAlreadyAsync(Expression<Func<FAM.Domain.Entities.AssetMaster.AssetAmc, bool>> predicate)
        {
            return await _applicationDbContext.AssetAmc.FirstOrDefaultAsync(predicate);
        }

        public async Task<int> UpdateAsync(int id, FAM.Domain.Entities.AssetMaster.AssetAmc assetAmc)
        {
                var existingamc = await _applicationDbContext.AssetAmc.FirstOrDefaultAsync(u => u.Id == id);

                // If the assetGroup does not exist
                if (existingamc is null)
                {
                    return -1; //indicate failure
                }

                // Update the existing assetGroup properties
                existingamc.StartDate = assetAmc.StartDate;
                existingamc.EndDate=assetAmc.EndDate;
                existingamc.Period = assetAmc.Period;
                existingamc.VendorCode = assetAmc.VendorCode;

                existingamc.VendorName = assetAmc.VendorName;
                existingamc.VendorPhone = assetAmc.VendorPhone;
                existingamc.VendorEmail = assetAmc.VendorEmail;

                existingamc.CoverageType = assetAmc.CoverageType;
                existingamc.FreeServiceCount = assetAmc.FreeServiceCount;
                existingamc.RenewalStatus = assetAmc.RenewalStatus;

                existingamc.RenewedDate = assetAmc.RenewedDate ?? null;
                existingamc.IsActive = assetAmc.IsActive;

                // Mark the entity as modified
                _applicationDbContext.AssetAmc.Update(existingamc);

                // Save changes to the database
                await _applicationDbContext.SaveChangesAsync();

                return 1; // Indicate success
        
        }

   
    }
}