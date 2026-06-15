namespace FinanceManagement.Application.Common.Interfaces.IMiscMaster
{
    public interface IMiscMasterCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.MiscMaster entity);
        Task<int> UpdateAsync(Domain.Entities.MiscMaster entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
        Task<int> GetMaxSortOrderAsync(int miscTypeId);
    }
}
