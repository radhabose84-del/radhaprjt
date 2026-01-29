using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetWarranty;
using FAM.Domain.Common;
using FAM.Domain.Entities.AssetMaster;
using FAM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Repositories.AssetMaster.AssetWarranty
{
    public class AssetWarrantyCommandRepository : IAssetWarrantyCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;        
        public AssetWarrantyCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;            
        }
        public async Task<AssetWarranties> CreateAsync(AssetWarranties assetWarranties)
        {            
            await _applicationDbContext.AssetWarranties.AddAsync(assetWarranties);
            await _applicationDbContext.SaveChangesAsync();
            return assetWarranties;          
        }
        public async Task<int> DeleteAsync(int depGroupId, AssetWarranties assetWarranties)
        {
            var assetSpecToDelete = await _applicationDbContext.AssetWarranties.FirstOrDefaultAsync(u => u.Id == depGroupId);
            if (assetSpecToDelete != null)
            {
                assetSpecToDelete.IsDeleted = assetWarranties.IsDeleted;              
                return await _applicationDbContext.SaveChangesAsync();
            }
            return 0;
        }
        public async Task<bool> UpdateAsync( AssetWarranties assetWarranties)
        {
            var existingAssetWarrantyGroup = await _applicationDbContext.AssetWarranties.FirstOrDefaultAsync(u => u.Id == assetWarranties.Id);             
    
            if (existingAssetWarrantyGroup != null)
            {
                existingAssetWarrantyGroup.AssetId = assetWarranties.AssetId;
                existingAssetWarrantyGroup.StartDate = assetWarranties.StartDate;                
                existingAssetWarrantyGroup.EndDate = assetWarranties.EndDate;
                existingAssetWarrantyGroup.IsActive = assetWarranties.IsActive;
                existingAssetWarrantyGroup.Period = assetWarranties.Period;
                existingAssetWarrantyGroup.WarrantyType = assetWarranties.WarrantyType;
                existingAssetWarrantyGroup.Description = assetWarranties.Description;
                existingAssetWarrantyGroup.ContactPerson = assetWarranties.ContactPerson;
                existingAssetWarrantyGroup.MobileNumber = assetWarranties.MobileNumber;
                existingAssetWarrantyGroup.Email = assetWarranties.Email;
                existingAssetWarrantyGroup.ServiceCountryId = assetWarranties.ServiceCountryId;
                existingAssetWarrantyGroup.ServiceStateId = assetWarranties.ServiceStateId;
                existingAssetWarrantyGroup.ServiceCityId = assetWarranties.ServiceCityId;
                existingAssetWarrantyGroup.ServiceAddressLine1 = assetWarranties.ServiceAddressLine1;
                existingAssetWarrantyGroup.ServiceAddressLine2 = assetWarranties.ServiceAddressLine2;
                existingAssetWarrantyGroup.ServicePinCode = assetWarranties.ServicePinCode;
                existingAssetWarrantyGroup.ServiceMobileNumber = assetWarranties.ServiceMobileNumber;
                existingAssetWarrantyGroup.ServiceEmail = assetWarranties.ServiceEmail;
                existingAssetWarrantyGroup.ServiceClaimProcessDescription = assetWarranties.ServiceClaimProcessDescription;
                existingAssetWarrantyGroup.ServiceLastClaimDate = assetWarranties.ServiceLastClaimDate;
                existingAssetWarrantyGroup.ServiceClaimStatus = assetWarranties.ServiceClaimStatus;
                _applicationDbContext.AssetWarranties.Update(existingAssetWarrantyGroup);
                return await _applicationDbContext.SaveChangesAsync()>0;
            }
           return false; 
        }
        public async Task<bool> ExistsByAssetIdAsync(int? assetCode)
        {
            return await _applicationDbContext.AssetWarranties.AnyAsync(c => c.AssetId == assetCode && c.IsActive== BaseEntity.Status.Active && c.IsDeleted==BaseEntity.IsDelete.NotDeleted );
        }      
         public async Task<AssetWarrantyDTO?> GetByAssetCodeAsync(string assetCode)
        {
          // return await _applicationDbContext.AssetWarranties
          // .FirstOrDefaultAsync(a => a.AssetId == assetId && a.IsDeleted == BaseEntity.IsDelete.NotDeleted && a.IsActive == BaseEntity.Status.Active );   

            var assetWarranty = await _applicationDbContext.AssetWarranties
            .Where(a => a.AssetMasterId.AssetCode == assetCode 
                && a.IsDeleted == BaseEntity.IsDelete.NotDeleted 
                && a.IsActive == BaseEntity.Status.Active)
                .Select(a => new AssetWarrantyDTO
                {
                    AssetId = a.AssetId,
                    AssetCode = a.AssetMasterId.AssetCode,
                    Id=a.Id                       
                }).FirstOrDefaultAsync();
            return assetWarranty;        
        }
        public async Task<bool> UpdateAssetWarrantyImageAsync(int assetId, string imageName)
        {
            var asset = await _applicationDbContext.AssetWarranties.FindAsync(assetId);
            if (asset == null)
            {
                return false;  // Asset not found
            }
            // Store only relative path (e.g., "HomeTextile/HomeTextile-COMP-MOU-1.png")
            asset.Document = imageName.Replace(@"\", "/"); 

            asset.Document = imageName;
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }
       public async Task<AssetWarrantyDTO?> GetByAssetWarrantyAsync(string? assetCode)
        {
            return await _applicationDbContext.AssetWarranties
                .Where(a => a.AssetMasterId.AssetCode == assetCode) // ✅ Correct lookup
                .Select(a => new AssetWarrantyDTO
                {
                    AssetCode = a.AssetMasterId.AssetCode,
                    Document = a.Document,  // ✅ Ensure correct document path
                    Id = a.Id,
                })
                .FirstOrDefaultAsync();
        }

      public async Task<bool> RemoveAssetWarrantyAsync(string imageName)
        {
            var asset = await _applicationDbContext.AssetWarranties.FirstOrDefaultAsync(x => x.Document == imageName);            
            if (asset == null)
            {
                return false;  // Asset not found
            }

            asset.Document = null;  // ✅ Remove document reference
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }

    }
}