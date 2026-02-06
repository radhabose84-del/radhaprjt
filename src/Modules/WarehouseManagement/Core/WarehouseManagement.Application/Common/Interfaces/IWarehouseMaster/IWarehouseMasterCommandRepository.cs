using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster
{
    public interface IWarehouseMasterCommandRepository
    {
        Task<int> CreateAsync(WarehouseManagement.Domain.Entities.WarehouseMaster warehouseMaster);
        Task<int> UpdateAsync(WarehouseManagement.Domain.Entities.WarehouseMaster warehouseMaster);
        Task<bool> DeleteAsync(int id, WarehouseManagement.Domain.Entities.WarehouseMaster warehouseMaster);
        Task<WarehouseManagement.Domain.Entities.WarehouseMaster?> GetByIdAsync(int id);
        


    }
}