namespace SalesManagement.Application.Common.Interfaces.IDiscountMaster
{
    public interface IDiscountMasterCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.DiscountMaster entity);
        Task<int> UpdateAsync(Domain.Entities.DiscountMaster entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
