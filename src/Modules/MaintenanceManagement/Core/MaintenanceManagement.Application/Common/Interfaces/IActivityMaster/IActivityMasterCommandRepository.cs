using MaintenanceManagement.Application.ActivityMaster.Command.UpdateActivityMster;

namespace MaintenanceManagement.Application.Common.Interfaces.IActivityMaster
{
    public interface IActivityMasterCommandRepository
    {
        Task<MaintenanceManagement.Domain.Entities.ActivityMaster> CreateAsync(MaintenanceManagement.Domain.Entities.ActivityMaster  activityMaster); 

      //  Task<MaintenanceManagement.Domain.Entities.ActivityMaster> UpdateAsync(MaintenanceManagement.Domain.Entities.ActivityMaster activityMaster);  
       //  Task<bool> UpdateAsync( MaintenanceManagement.Domain.Entities.ActivityMaster activityMaster);

         Task<int> UpdateAsync(UpdateActivityMasterDto activityMaster);
    }
}