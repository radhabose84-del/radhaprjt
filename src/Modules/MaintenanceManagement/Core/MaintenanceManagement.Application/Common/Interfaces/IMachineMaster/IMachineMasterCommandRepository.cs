namespace MaintenanceManagement.Application.Common.Interfaces.IMachineMaster
{
    public interface IMachineMasterCommandRepository
    {
        Task<int> CreateAsync(MaintenanceManagement.Domain.Entities.MachineMaster machineMaster);
        Task<bool> UpdateAsync(int Id, MaintenanceManagement.Domain.Entities.MachineMaster machineMaster);
        Task<bool> DeleteAsync(int Id, MaintenanceManagement.Domain.Entities.MachineMaster machineMaster);
        Task<bool> IsNameDuplicateAsync(string? name,int machineGroupId, int excludeId);
        Task<bool> ExistsByCodeAsync(string? MachineCode);
        Task<bool> IsCodeDuplicateAsync(string? code,int unitId, int excludeId);
    }
}