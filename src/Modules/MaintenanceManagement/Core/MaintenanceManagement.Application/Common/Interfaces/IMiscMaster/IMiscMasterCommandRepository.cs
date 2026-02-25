namespace MaintenanceManagement.Application.Common.Interfaces.IMiscMaster
{
    public interface IMiscMasterCommandRepository
    {
      Task<MaintenanceManagement.Domain.Entities.MiscMaster> CreateAsync(MaintenanceManagement.Domain.Entities.MiscMaster miscMaster);  

       Task<int> GetMaxSortOrderAsync();

       Task<bool> UpdateAsync(int id, MaintenanceManagement.Domain.Entities.MiscMaster miscMaster);

        Task<bool> DeleteAsync(int id,MaintenanceManagement.Domain.Entities.MiscMaster miscMaster);         

    }
}