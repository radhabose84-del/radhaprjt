using Contracts.Dtos.Inventory;

namespace Contracts.Interfaces.External.IInvetoryManagement
{
    public interface IItemGroupGrpcClient
    {
         Task<List<ItemGroupDto>> GetAllItemGroupsAsync();
    }
}