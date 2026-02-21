#nullable disable

namespace SalesManagement.Application.Common.Interfaces.ISalesSegment
{
    public interface ISalesSegmentCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.SalesSegment entity);
        Task<int> UpdateAsync(Domain.Entities.SalesSegment entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
