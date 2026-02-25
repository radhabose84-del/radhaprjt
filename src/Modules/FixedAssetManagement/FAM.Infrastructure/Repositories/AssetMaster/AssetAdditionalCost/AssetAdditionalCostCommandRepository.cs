using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAdditionalCost;
using FAM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Repositories.AssetMaster.AssetAdditionalCost
{
    public class AssetAdditionalCostCommandRepository:IAssetAdditionalCostCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
         
        public AssetAdditionalCostCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost assetAdditionalCost)
        {
        // Add the AssetAdditionalCost to the DbContext
        await _applicationDbContext.AssetAdditionalCost.AddAsync(assetAdditionalCost);

        // Save changes to the database
        await _applicationDbContext.SaveChangesAsync();

        // Return the ID of the created AssetAdditionalCost
        return assetAdditionalCost.Id;
        }

        public async Task<int> UpdateAsync(int id,FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost assetAdditionalCost)
        {
        var existingassetcost = await _applicationDbContext.AssetAdditionalCost.FirstOrDefaultAsync(u => u.Id == id);

        // If the assetGroup does not exist
        if (existingassetcost is null)
        {
            return -1; //indicate failure
        }

        // Update the existing assetGroup properties
        existingassetcost.Amount = assetAdditionalCost.Amount;
        existingassetcost.JournalNo = assetAdditionalCost.JournalNo;
        existingassetcost.CostType = assetAdditionalCost.CostType;

        // Mark the entity as modified
        _applicationDbContext.AssetAdditionalCost.Update(existingassetcost);

        // Save changes to the database
        await _applicationDbContext.SaveChangesAsync();

        return 1; // Indicate success
        }
    }
}