using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryManagement.Application.Common.Interfaces.IUOM
{
    public interface IUOMCommandRepository
    {
         Task<InventoryManagement.Domain.Entities.UOM> CreateAsync(InventoryManagement.Domain.Entities.UOM uom);     
        Task<bool> UpdateAsync(InventoryManagement.Domain.Entities.UOM uom);
        Task<bool> DeleteAsync(int id, InventoryManagement.Domain.Entities.UOM uom); 
         Task<int> GetMaxSortOrderAsync();
         Task<bool> CheckForDuplicatesAsync(string name, int sortOrder, int excludeId);   
    }
}