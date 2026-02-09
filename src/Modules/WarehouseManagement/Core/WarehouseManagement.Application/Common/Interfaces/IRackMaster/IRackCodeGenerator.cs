using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WarehouseManagement.Application.Common.Interfaces.IRackMaster
{
    public interface IRackCodeGenerator
    {
         Task<string> GenerateAsync(int warehouseId, int? floorId, int? aisleId, int? rackLevelId);
    }
}