using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Contracts.Dtos.Inventory;

namespace Contracts.Interfaces.External.IInvetoryManagement
{
    public interface IItemGrpcClient
    {
        Task<List<ItemMasterDto>> GetItemAutoCompleteAsync(string search,int? itemGroupId, int? itemCategoryId,int? sourceId,int? issueRuleId, CancellationToken ct = default);
        Task<List<ItemMasterDto>> GetItemsByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
         
         
    }
}
