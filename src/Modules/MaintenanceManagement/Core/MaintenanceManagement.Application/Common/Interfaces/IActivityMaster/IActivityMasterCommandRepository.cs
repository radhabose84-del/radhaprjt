using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.ActivityMaster.Command.UpdateActivityMster;

namespace MaintenanceManagement.Application.Common.Interfaces.IActivityMaster
{
    public interface IActivityMasterCommandRepository
    {
        Task<MaintenanceManagement.Domain.Entities.ActivityMaster> CreateAsync(MaintenanceManagement.Domain.Entities.ActivityMaster  activityMaster); 

      //  Task<Core.Domain.Entities.ActivityMaster> UpdateAsync(Core.Domain.Entities.ActivityMaster activityMaster);  
       //  Task<bool> UpdateAsync( Core.Domain.Entities.ActivityMaster activityMaster);

         Task<int> UpdateAsync(UpdateActivityMasterDto activityMaster);
    }
}