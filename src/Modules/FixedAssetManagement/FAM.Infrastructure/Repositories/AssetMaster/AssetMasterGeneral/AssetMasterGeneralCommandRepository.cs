using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Repositories.AssetMaster.AssetMasterGeneral
{
    public class AssetMasterGeneralCommandRepository : IAssetMasterGeneralCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;        
        public AssetMasterGeneralCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;            
        }
        public async Task<AssetMasterGenerals> CreateAsync(AssetMasterGenerals assetMasterGeneral, CancellationToken cancellationToken)
        {
           var entry =_applicationDbContext.Entry(assetMasterGeneral);
            await _applicationDbContext.AssetMasterGenerals.AddAsync(assetMasterGeneral);
            await _applicationDbContext.SaveChangesAsync();
             return assetMasterGeneral;   
        }

        public async Task<bool> DeleteAsync(int Id, AssetMasterGenerals assetMaster)
        {
            var assetMasterToDelete = await _applicationDbContext.AssetMasterGenerals.FirstOrDefaultAsync(u => u.Id == Id);
            if (assetMasterToDelete != null)
            {
                assetMasterToDelete.IsDeleted = assetMaster.IsDeleted;              
                return await _applicationDbContext.SaveChangesAsync()>0;
            }
            return false;
        }
        public async Task<bool> UpdateAsync(int assetId,AssetMasterGenerals assetMaster)
        {
            var existingAssetGroup = await _applicationDbContext.AssetMasterGenerals
                   .Include(cf => cf.AssetAdditionalCost)
                   .Include(cf => cf.AssetPurchase)
                   .Include(cf => cf.AssetLocation)
                   .FirstOrDefaultAsync(u => u.Id ==assetId);

               if (existingAssetGroup == null)
                   return false;

               
               _applicationDbContext.AssetAdditionalCost.RemoveRange(
                   _applicationDbContext.AssetAdditionalCost.Where(x => x.AssetId == assetId));

               _applicationDbContext.AssetPurchaseDetails.RemoveRange(
                   _applicationDbContext.AssetPurchaseDetails.Where(x => x.AssetId == assetId));

               _applicationDbContext.AssetLocations.RemoveRange(
                   _applicationDbContext.AssetLocations.Where(x => x.AssetId == assetId));

                       
                existingAssetGroup.AssetName = assetMaster.AssetName;                
                existingAssetGroup.AssetGroupId = assetMaster.AssetGroupId;
                existingAssetGroup.IsActive = assetMaster.IsActive;                
                existingAssetGroup.AssetSubGroupId = assetMaster.AssetSubGroupId;
                existingAssetGroup.AssetCategoryId = assetMaster.AssetCategoryId;
                existingAssetGroup.AssetSubCategoryId = assetMaster.AssetSubCategoryId;
                existingAssetGroup.AssetParentId = assetMaster.AssetParentId;
                existingAssetGroup.AssetType = assetMaster.AssetType;
                existingAssetGroup.MachineCode = assetMaster.MachineCode;
                existingAssetGroup.Quantity = assetMaster.Quantity;
                existingAssetGroup.UOMId = assetMaster.UOMId;
                existingAssetGroup.AssetDescription = assetMaster.AssetDescription;
                existingAssetGroup.WorkingStatus = assetMaster.WorkingStatus;
                existingAssetGroup.AssetImage = assetMaster.AssetImage;
                existingAssetGroup.ISDepreciated = assetMaster.ISDepreciated;
                existingAssetGroup.IsTangible = assetMaster.IsTangible;            
                existingAssetGroup.IsActive = BaseEntity.Status.Active;    
                existingAssetGroup.PutToUseDate = assetMaster.PutToUseDate;

                if (assetMaster.AssetAdditionalCost?.Any() == true)
                {
                    foreach (var cost in assetMaster.AssetAdditionalCost)
                    {
                        cost.AssetId = assetId;
                    }

                    await _applicationDbContext.AssetAdditionalCost.AddRangeAsync(assetMaster.AssetAdditionalCost);
                }

              if (assetMaster.AssetPurchase?.Any() == true)
                {
                    foreach (var purchaseDetail in assetMaster.AssetPurchase)
                    {
                        purchaseDetail.QcCompleted = 'Y';
                        purchaseDetail.AssetId = assetId;
                    }
                    
                    await _applicationDbContext.AssetPurchaseDetails.AddRangeAsync(assetMaster.AssetPurchase);
                }
             /*   if (assetMaster.AssetLocation?.Any() == true)
                   await _applicationDbContext.AssetLocations.AddRangeAsync(assetMaster.AssetLocation); */
                if (assetMaster.AssetLocation != null)
                {
                    assetMaster.AssetLocation.AssetId = assetId; 
                    await _applicationDbContext.AssetLocations.AddAsync(assetMaster.AssetLocation);
                }
               return await _applicationDbContext.SaveChangesAsync() > 0;
          
        }     
        public async Task<AssetMasterGenerals?> GetByAssetCodeAsync(string assetCode)
        {
            return await _applicationDbContext.AssetMasterGenerals
                .FirstOrDefaultAsync(a => a.AssetCode == assetCode && a.IsDeleted == BaseEntity.IsDelete.NotDeleted && a.IsActive == BaseEntity.Status.Active );   
        }
        public async Task<bool> UpdateAssetImageAsync(int assetId, string imageName)
        {
            var asset = await _applicationDbContext.AssetMasterGenerals.FindAsync(assetId);
            if (asset == null)
            {
                return false;  // Asset not found
            }
            // Store only relative path (e.g., "HomeTextile/HomeTextile-COMP-MOU-1.png")
           // asset.AssetImage = imageName.Replace(@"\", "/"); 

            asset.AssetImage = imageName;
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UpdateAssetDocumentAsync(int assetId, string imageName)
        {
            var asset = await _applicationDbContext.AssetMasterGenerals.FindAsync(assetId);
            if (asset == null)
            {
                return false;  
            }
            asset.AssetDocument = imageName;
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }
        public async Task<AssetMasterGeneralDTO?> GetByAssetImageAsync(string assetCode)
        {
           /*  return await _applicationDbContext.AssetMasterGenerals
                .FirstOrDefaultAsync(a => a.AssetImage == assetCode); */
            return await _applicationDbContext.AssetMasterGenerals
            .Where(a => a.AssetCode == assetCode)
            .Select(a => new AssetMasterGeneralDTO
            {
                AssetCode = a.AssetCode,
                AssetImage = a.AssetImage,
                Id = a.Id,
            })
            .FirstOrDefaultAsync();
        }

        public async Task<bool> RemoveAssetImageReferenceAsync(string imageName)
        {
            var asset = await _applicationDbContext.AssetMasterGenerals.FirstOrDefaultAsync(x => x.AssetImage == imageName);
            if (asset == null)
            {
                return false;  // Asset not found
            }
            asset.AssetImage = null;
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveAssetDocumentReferenceAsync(string imageName)
        {
            var asset = await _applicationDbContext.AssetMasterGenerals.FirstOrDefaultAsync(x => x.AssetDocument == imageName);
            if (asset == null)
            {
                return false;  // Asset not found
            }
            asset.AssetDocument = null;
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveAssetWarrantyAsync(string assetPath)
        {
           var asset = await _applicationDbContext.AssetWarranties.FirstOrDefaultAsync(x => x.Document == assetPath);
            if (asset == null)
            {
                return false;  // Asset not found
            }
            asset.Document = null;
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UpdateDocumentAsync(int AssetId, string imageName)
        {
            var assetDocument = await _applicationDbContext.AssetMasterGenerals.FindAsync(AssetId);
            if (assetDocument == null)
            {
                return false;  
            }          
            assetDocument.AssetDocument = imageName;
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }
    }
}