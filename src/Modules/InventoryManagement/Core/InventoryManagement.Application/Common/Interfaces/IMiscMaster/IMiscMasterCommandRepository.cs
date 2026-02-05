using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryManagement.Application.Common.Interfaces.IMiscMaster
{
    public interface IMiscMasterCommandRepository
    {
        Task<InventoryManagement.Domain.Entities.MiscMaster> CreateAsync(InventoryManagement.Domain.Entities.MiscMaster miscMaster);  

        Task<int> GetMaxSortOrderAsync();

        Task<bool> UpdateAsync(int id, InventoryManagement.Domain.Entities.MiscMaster miscMaster);

        Task<bool> DeleteAsync(int id,InventoryManagement.Domain.Entities.MiscMaster miscMaster);  
    }
}