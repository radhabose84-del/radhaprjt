namespace ProductionManagement.Application.Common.Interfaces.IPackType
{
    public interface IPackTypeCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.PackType entity);
        Task<int> UpdateAsync(Domain.Entities.PackType entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
