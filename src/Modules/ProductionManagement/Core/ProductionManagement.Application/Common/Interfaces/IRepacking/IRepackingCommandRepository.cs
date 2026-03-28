using ProductionManagement.Domain.Entities;

namespace ProductionManagement.Application.Common.Interfaces.IRepacking
{
    public interface IRepackingCommandRepository
    {
        Task<int> CreateAsync(RepackingHeader entity, int typeId);
        Task<int> UpdateAsync(RepackingHeader entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
