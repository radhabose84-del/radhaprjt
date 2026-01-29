using FAM.Domain.Entities;

namespace FAM.Application.Common.Interfaces.ISpecificationMaster
{
    public interface ISpecificationMasterCommandRepository
    {
        Task<SpecificationMasters> CreateAsync(SpecificationMasters specificationMaster);
        Task<int>  UpdateAsync(SpecificationMasters specificationMaster);
        Task<int>  DeleteAsync(int specId,SpecificationMasters specificationMaster); 
        Task<bool> ExistsByAssetGroupIdAsync(int? assetGroupId,string? specificationName); // ✅ New method       
    }
}