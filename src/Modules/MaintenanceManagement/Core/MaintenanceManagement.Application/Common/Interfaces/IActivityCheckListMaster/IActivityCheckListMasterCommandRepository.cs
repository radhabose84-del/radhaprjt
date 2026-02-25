namespace MaintenanceManagement.Application.Common.Interfaces.IActivityCheckListMaster
{
    public interface IActivityCheckListMasterCommandRepository
    {
        Task<MaintenanceManagement.Domain.Entities.ActivityCheckListMaster> CreateAsync(MaintenanceManagement.Domain.Entities.ActivityCheckListMaster activityCheckListMaster);

        Task<bool> UpdateAsync(int id, MaintenanceManagement.Domain.Entities.ActivityCheckListMaster activityCheckListMaster);
           

       Task<bool> DeleteAsync(int Id,MaintenanceManagement.Domain.Entities.ActivityCheckListMaster activityCheckListMaster);

    }
}