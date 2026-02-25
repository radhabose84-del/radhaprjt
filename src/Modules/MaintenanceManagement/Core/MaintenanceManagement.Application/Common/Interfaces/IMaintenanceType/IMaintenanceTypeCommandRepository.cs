namespace MaintenanceManagement.Application.Common.Interfaces.IMaintenanceType
{
    public interface IMaintenanceTypeCommandRepository
    {
        Task<int> CreateAsync(MaintenanceManagement.Domain.Entities.MaintenanceType maintenanceType);
        Task<int> UpdateAsync(int Id,MaintenanceManagement.Domain.Entities.MaintenanceType maintenanceType);
        Task<int> DeleteAsync(int Id,MaintenanceManagement.Domain.Entities.MaintenanceType maintenanceType);
        Task<bool> IsNameDuplicateAsync(string? name, int excludeId);
        Task<bool> ExistsByCodeAsync(string? TypeName);
    }
}