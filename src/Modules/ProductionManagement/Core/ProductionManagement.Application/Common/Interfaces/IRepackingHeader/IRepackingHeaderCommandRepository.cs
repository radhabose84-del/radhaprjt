namespace ProductionManagement.Application.Common.Interfaces.IRepackingHeader
{
    public interface IRepackingHeaderCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.RepackingHeader entity, int typeId);
        Task<int> UpdateAsync(Domain.Entities.RepackingHeader entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
