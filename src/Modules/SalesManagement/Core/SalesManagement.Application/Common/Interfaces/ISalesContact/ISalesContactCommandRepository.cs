namespace SalesManagement.Application.Common.Interfaces.ISalesContact
{
    public interface ISalesContactCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.SalesContact entity);
        Task<int> UpdateAsync(Domain.Entities.SalesContact entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
