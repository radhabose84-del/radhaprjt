namespace FAM.Application.Common.Interfaces.IAssetGroup
{
    public interface IAssetGroupQueryRepository
    {
        Task<FAM.Domain.Entities.AssetGroup?> GetByIdAsync(int Id);
        Task<(List<FAM.Domain.Entities.AssetGroup>, int)> GetAllAssetGroupAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<List<FAM.Domain.Entities.AssetGroup>> GetAssetGroups(string searchPattern);
        Task<List<FAM.Domain.Entities.AssetGroup>> GetByIdsAsync(IEnumerable<int> ids);
        Task<bool> IsAssetGroupLinkedAsync(int assetGroupId); //IsActive And Delete Validation 
     
    }
}