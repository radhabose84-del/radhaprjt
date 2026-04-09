namespace SalesManagement.Application.Common.Interfaces.ICommissionSplit
{
    public interface ICommissionSplitCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.CommissionSplit entity);
        Task<int> UpdateAsync(Domain.Entities.CommissionSplit entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
