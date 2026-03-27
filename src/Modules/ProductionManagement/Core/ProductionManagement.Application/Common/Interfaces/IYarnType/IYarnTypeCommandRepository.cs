namespace ProductionManagement.Application.Common.Interfaces.IYarnType
{
    public interface IYarnTypeCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.YarnType entity);
        Task<int> UpdateAsync(Domain.Entities.YarnType entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
