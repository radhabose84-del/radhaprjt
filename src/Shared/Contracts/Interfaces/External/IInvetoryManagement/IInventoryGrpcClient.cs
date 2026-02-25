using Contracts.Dtos.Inventory;

namespace Contracts.Interfaces.External.IInvetoryManagement
{
    public interface IInventoryGrpcClient
    {      
         
          Task<IReadOnlyList<InventoryQueryDto>> GetItemPurchaseToleranceAsync( IEnumerable<int> itemIds, CancellationToken ct = default);
    }
}