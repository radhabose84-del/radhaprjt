using Contracts.Dtos.Inventory;

namespace Contracts.Interfaces.External.IInvetoryManagement
{
    public interface IInventoryCategoryGrpcClient
    {
        Task<List<CategoryMasterDto>> GetCategoryByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    }
}