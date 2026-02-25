using FAM.Application.Common.Interfaces.IAssetMaster.IAssetDisposal;
using FAM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Repositories.AssetMaster.AssetDisposal
{
    public class AssetDisposalCommandRepository : IAssetDisposalCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
         
        public AssetDisposalCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(FAM.Domain.Entities.AssetMaster.AssetDisposal assetDisposal)
        {
            // Add the AssetDisposal to the DbContext
            await _applicationDbContext.AssetDisposal.AddAsync(assetDisposal);

            // Save changes to the database
            await _applicationDbContext.SaveChangesAsync();

            // Return the ID of the created AssetDisposal
            return assetDisposal.Id;
        }

        public async Task<int> UpdateAsync(int id, FAM.Domain.Entities.AssetMaster.AssetDisposal assetDisposal)
        {
             var existingassetdisposal = await _applicationDbContext.AssetDisposal.FirstOrDefaultAsync(u => u.Id == id);

        // If the assetGroup does not exist
        if (existingassetdisposal is null)
        {
            return -1; //indicate failure
        }

        // Update the existing assetGroup properties
        existingassetdisposal.DisposalDate = assetDisposal.DisposalDate;
        existingassetdisposal.DisposalType = assetDisposal.DisposalType;
        existingassetdisposal.DisposalReason = assetDisposal.DisposalReason;
        existingassetdisposal.DisposalAmount = assetDisposal.DisposalAmount;

        // Mark the entity as modified
        _applicationDbContext.AssetDisposal.Update(existingassetdisposal);

        // Save changes to the database
        await _applicationDbContext.SaveChangesAsync();

        return 1; // Indicate success
        }
    }
}