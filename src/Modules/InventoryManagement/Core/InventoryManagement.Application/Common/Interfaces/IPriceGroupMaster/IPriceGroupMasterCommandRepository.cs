namespace InventoryManagement.Application.Common.Interfaces.IPriceGroupMaster
{
    public interface IPriceGroupMasterCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.PriceGroupMaster entity);

        Task<int> UpdateAsync(Domain.Entities.PriceGroupMaster entity);

        Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken);
    }
}
