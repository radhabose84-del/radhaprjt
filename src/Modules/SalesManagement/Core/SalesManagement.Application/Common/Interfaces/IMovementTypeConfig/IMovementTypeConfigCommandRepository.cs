namespace SalesManagement.Application.Common.Interfaces.IMovementTypeConfig
{
    public interface IMovementTypeConfigCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.MovementTypeConfig entity);
        Task<int> UpdateAsync(Domain.Entities.MovementTypeConfig entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
