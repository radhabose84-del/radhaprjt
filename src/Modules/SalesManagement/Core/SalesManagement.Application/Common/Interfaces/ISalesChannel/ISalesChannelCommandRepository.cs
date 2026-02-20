#nullable disable
namespace SalesManagement.Application.Common.Interfaces.ISalesChannel
{
    public interface ISalesChannelCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.SalesChannel entity);
        Task<int> UpdateAsync(Domain.Entities.SalesChannel entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
