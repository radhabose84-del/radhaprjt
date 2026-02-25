namespace MaintenanceManagement.Application.Common.Interfaces.Power.IFeederGroup
{
    public interface IFeederGroupCommandRepository
    {

        Task<int> CreateAsync(MaintenanceManagement.Domain.Entities.Power.FeederGroup feederGroup);

        Task<bool> UpdateAsync(int id, MaintenanceManagement.Domain.Entities.Power.FeederGroup feederGroup);
        
         Task<bool> DeleteAsync(int id,MaintenanceManagement.Domain.Entities.Power.FeederGroup feederGroup); 


        
    }
}