using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Inventory;

namespace Contracts.Interfaces.External.IInvetoryManagement
{
    public interface IPutawayRuleGrpcClient
    {
        Task<List<PutawayRuleDto>> GetPutAwayRuleDetailsAsync(List<int> itemIds,List<int> warehouseIds,CancellationToken cancellationToken);
        Task<List<PutawayRuleDto>> GetPutAwayRuleDetailsByWarehouseAsync(
            List<int> itemIds,
            List<int> warehouseIds,
            CancellationToken cancellationToken);
        
    }
}