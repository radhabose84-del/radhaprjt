using FAM.Application.Common.Interfaces.ISpecificationMaster;
using FAM.Domain.Entities;
using FAM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Infrastructure.Repositories.SpecificationMaster
{
    public class SpecificationMasterCommandRepository : ISpecificationMasterCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;        
        public SpecificationMasterCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;            
        }
        public async Task<SpecificationMasters> CreateAsync(SpecificationMasters specificationMaster)
        {           
            await _applicationDbContext.SpecificationMasters.AddAsync(specificationMaster);
            await _applicationDbContext.SaveChangesAsync();
            return specificationMaster;          
        }
        public async Task<int> DeleteAsync(int specId, SpecificationMasters specificationMaster)
        {
            var SepGroupToDelete = await _applicationDbContext.SpecificationMasters.FirstOrDefaultAsync(u => u.Id == specId);
            if (SepGroupToDelete != null)
            {
                SepGroupToDelete.IsDeleted = specificationMaster.IsDeleted;              
                return await _applicationDbContext.SaveChangesAsync();
            }
            return 0;
        }
        public async Task<int> UpdateAsync(SpecificationMasters specificationMaster)
        {
            var existingSepGroup = await _applicationDbContext.SpecificationMasters.FirstOrDefaultAsync(u => u.Id == specificationMaster.Id);             
    
            if (existingSepGroup != null)
            {
                existingSepGroup.AssetGroupId = specificationMaster.AssetGroupId;
                existingSepGroup.SpecificationName = specificationMaster.SpecificationName;                
                existingSepGroup.ISDefault = specificationMaster.ISDefault;
                existingSepGroup.IsActive = specificationMaster.IsActive;               
                _applicationDbContext.SpecificationMasters.Update(existingSepGroup);
                return await _applicationDbContext.SaveChangesAsync();  
            }
           return 0; 
        }
        public async Task<bool> ExistsByAssetGroupIdAsync(int? assetGroupId,string? specificationName)
        {
            return await _applicationDbContext.SpecificationMasters.AnyAsync(c => c.SpecificationName == specificationName && c.AssetGroupId == assetGroupId && c.IsDeleted == IsDelete.NotDeleted); ;
        }
      
    }
}