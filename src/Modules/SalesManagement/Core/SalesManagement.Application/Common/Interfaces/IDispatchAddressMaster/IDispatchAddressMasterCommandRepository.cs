namespace SalesManagement.Application.Common.Interfaces.IDispatchAddressMaster
{
    public interface IDispatchAddressMasterCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.DispatchAddressMaster entity);
        Task<int> UpdateAsync(Domain.Entities.DispatchAddressMaster entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
