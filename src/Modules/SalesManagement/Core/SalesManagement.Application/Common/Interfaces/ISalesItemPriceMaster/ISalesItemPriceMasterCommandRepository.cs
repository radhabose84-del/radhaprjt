namespace SalesManagement.Application.Common.Interfaces.ISalesItemPriceMaster
{
    public interface ISalesItemPriceMasterCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.SalesItemPriceMaster entity);
        Task<int> UpdateAsync(Domain.Entities.SalesItemPriceMaster entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
