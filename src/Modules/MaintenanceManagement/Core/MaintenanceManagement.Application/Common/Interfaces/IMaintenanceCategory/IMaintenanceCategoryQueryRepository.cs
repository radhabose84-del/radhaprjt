namespace MaintenanceManagement.Application.Common.Interfaces.IMaintenanceCategory
{
    public interface IMaintenanceCategoryQueryRepository
    {
        Task<MaintenanceManagement.Domain.Entities.MaintenanceCategory?> GetByIdAsync(int Id);
        Task<(List<MaintenanceManagement.Domain.Entities.MaintenanceCategory>,int)> GetAllMaintenanceCategoryAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<List<MaintenanceManagement.Domain.Entities.MaintenanceCategory>> GetMaintenanceCategoryAsync(string searchPattern);
    }
}