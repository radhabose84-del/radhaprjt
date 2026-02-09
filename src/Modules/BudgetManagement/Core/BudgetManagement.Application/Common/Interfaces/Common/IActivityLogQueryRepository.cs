using BudgetManagement.Domain.Entities;

public interface IActivityLogQueryRepository
{
    Task<(List<ActivityLog> Items, int Total)> GetAllAsync(string entityName, int entityId, int page, int size, CancellationToken ct);
    Task<ActivityLog?> GetByIdAsync(long id, CancellationToken ct);
}
