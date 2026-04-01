namespace FAM.Application.Common.Interfaces.IAssetSubGroup
{
    public interface IAssetSubGroupQueryRepository
    {
        Task<FAM.Domain.Entities.AssetSubGroup?> GetByIdAsync(int Id);
        Task<List<FAM.Domain.Entities.AssetSubGroup?>> GetByGroupIdAsync(int GroupId);
        Task<(List<FAM.Domain.Entities.AssetSubGroup>, int)> GetAllAssetSubGroupAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<List<FAM.Domain.Entities.AssetSubGroup>> GetAssetSubGroups(string searchPattern);
        Task<bool> IsAssetSubGroupLinkedAsync(int id);
        Task<bool> SoftDeleteValidationAsync(int id);
    }
}