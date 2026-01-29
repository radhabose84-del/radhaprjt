using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Dtos.Inventory;

namespace Contracts.Interfaces.External.IInvetoryManagement
{
    public interface IItemGroupGrpcClient
    {
         Task<List<ItemGroupDto>> GetAllItemGroupsAsync();
    }
}