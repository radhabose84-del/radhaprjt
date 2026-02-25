namespace MaintenanceManagement.Application.Common.Interfaces.IMaintenanceType
{
    public interface IMaintenanceTypeQueryRepository
    {
         Task<MaintenanceManagement.Domain.Entities.MaintenanceType?> GetByIdAsync(int Id);
         Task<(List<MaintenanceManagement.Domain.Entities.MaintenanceType>,int)> GetAllMaintenanceTypeAsync(int PageNumber, int PageSize, string? SearchTerm);
         Task<List<MaintenanceManagement.Domain.Entities.MaintenanceType>> GetMaintenanceTypeAsync(string searchPattern);
    }
}