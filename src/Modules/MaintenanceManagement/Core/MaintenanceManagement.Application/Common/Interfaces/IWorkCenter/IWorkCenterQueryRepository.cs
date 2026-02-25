namespace MaintenanceManagement.Application.Common.Interfaces.IWorkCenter
{
    public interface IWorkCenterQueryRepository
    {
        Task<MaintenanceManagement.Domain.Entities.WorkCenter?> GetByIdAsync(int Id);
        Task<(List<MaintenanceManagement.Domain.Entities.WorkCenter>, int)> GetAllWorkCenterGroupAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<List<MaintenanceManagement.Domain.Entities.WorkCenter>> GetWorkCenterGroups(string searchPattern);
        Task<bool> SoftDeleteValidation(int Id); 
        Task<bool> IsWorkCenterLinkedAsync(int id);

    }
}