using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.Common.Interfaces.IMachineGroup
{
    public interface IMachineGroupCommandRepository 
    {

      Task<MaintenanceManagement.Domain.Entities.MachineGroup> CreateAsync(MaintenanceManagement.Domain.Entities.MachineGroup machineGroup); 
      Task<bool> UpdateAsync(int id, MaintenanceManagement.Domain.Entities.MachineGroup machineGroup);  
       Task<bool> DeleteAsync(int id,MaintenanceManagement.Domain.Entities.MachineGroup  machineGroup); 
  
            
    }
}