
namespace SalesManagement.Application.Common.Interfaces.ISalesGroup
{
    public interface ISalesGroupCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.SalesGroup entity);
        Task<int> UpdateAsync(Domain.Entities.SalesGroup entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
