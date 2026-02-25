namespace MaintenanceManagement.Application.Common.Interfaces.IMiscTypeMaster
{
    public interface IMiscTypeMasterCommandRepository  
    {
    Task<MaintenanceManagement.Domain.Entities.MiscTypeMaster> CreateAsync(MaintenanceManagement.Domain.Entities.MiscTypeMaster miscTypeMaster);   
    Task<bool> UpdateAsync(int id, MaintenanceManagement.Domain.Entities.MiscTypeMaster miscTypeMaster);
    Task<bool> DeleteAsync(int id,MaintenanceManagement.Domain.Entities.MiscTypeMaster miscTypeMaster); 


    }
}