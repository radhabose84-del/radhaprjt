namespace MaintenanceManagement.Application.Common.Interfaces.IMachineGroupUser
{
    public interface IMachineGroupUserCommandRepository
    {
        Task<int> CreateAsync(MaintenanceManagement.Domain.Entities.MachineGroupUser  machineGroupUser);     
        Task<bool> UpdateAsync(MaintenanceManagement.Domain.Entities.MachineGroupUser machineGroupUser);
        Task<bool> DeleteAsync(int id,MaintenanceManagement.Domain.Entities.MachineGroupUser machineGroupUser); 
    }
}