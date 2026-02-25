using FAM.Application.Common.Interfaces.IAssetMaster.IAssetSpecification;
using FAM.Domain.Common;
using FAM.Domain.Entities.AssetMaster;
using FAM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Repositories.AssetMaster.AssetSpecification
{
    public class AssetSpecificationCommandRepository : IAssetSpecificationCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;        
        public AssetSpecificationCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;            
        }
        public async Task<AssetSpecifications> CreateAsync(AssetSpecifications assetSpecifications)
        {            
            await _applicationDbContext.AssetSpecifications.AddAsync(assetSpecifications);
            await _applicationDbContext.SaveChangesAsync();
            return assetSpecifications;          
        }
        public async Task<int> DeleteAsync(int id, AssetSpecifications assetSpecifications)
        {
            var assetSpecList = await _applicationDbContext.AssetSpecifications
            .Where(u => u.AssetId == id)
            .ToListAsync();

            if (!assetSpecList.Any())
            {
                return -1; // No records found
            }
            _applicationDbContext.AssetSpecifications.RemoveRange(assetSpecList);

            return await _applicationDbContext.SaveChangesAsync();
        }
        public async Task<int> UpdateAsync(int assetId, AssetSpecifications assetSpecifications)
        {
            var existingAssetSpecGroup = await _applicationDbContext.AssetSpecifications
                .FirstOrDefaultAsync(u => u.AssetId == assetId && u.SpecificationId == assetSpecifications.SpecificationId);

                if (existingAssetSpecGroup != null)
                {
                    existingAssetSpecGroup.SpecificationValue = assetSpecifications.SpecificationValue;
                    existingAssetSpecGroup.IsActive = assetSpecifications.IsActive;

                    // No need to reassign AssetId and SpecificationId again
                    _applicationDbContext.AssetSpecifications.Update(existingAssetSpecGroup);
                    return await _applicationDbContext.SaveChangesAsync();
                }
           return 0; 
        }
        public async Task<bool> ExistsByAssetSpecIdAsync(int? assetId,int? assetSpecId)
        {
            return await _applicationDbContext.AssetSpecifications.AnyAsync(c => c.AssetId == assetId && c.SpecificationId == assetSpecId && c.IsDeleted==BaseEntity.IsDelete.NotDeleted );
        }

        public async Task<bool> ExistsByManufactureAsync(int? manufactureId)
        {
            return await _applicationDbContext.Manufactures.AnyAsync(c => c.Id == manufactureId && c.IsDeleted==BaseEntity.IsDelete.NotDeleted && c.IsActive==BaseEntity.Status.Active);
        }
    }
}