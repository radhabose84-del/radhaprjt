namespace SalesManagement.Application.Common.Interfaces.IDispatchAddressMapping
{
    public interface IDispatchAddressMappingCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.DispatchAddressMapping entity);
        Task<int> UpdateAsync(Domain.Entities.DispatchAddressMapping entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
