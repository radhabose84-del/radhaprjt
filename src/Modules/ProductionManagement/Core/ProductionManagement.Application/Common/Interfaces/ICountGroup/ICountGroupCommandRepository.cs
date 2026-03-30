namespace ProductionManagement.Application.Common.Interfaces.ICountGroup
{
    public interface ICountGroupCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.CountGroup entity);
        Task<int> UpdateAsync(Domain.Entities.CountGroup entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
