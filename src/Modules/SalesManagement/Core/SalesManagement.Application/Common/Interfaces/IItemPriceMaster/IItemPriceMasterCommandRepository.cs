namespace SalesManagement.Application.Common.Interfaces.IItemPriceMaster
{
    public interface IItemPriceMasterCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.ItemPriceMaster entity);
        Task<int> UpdateAsync(Domain.Entities.ItemPriceMaster entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
