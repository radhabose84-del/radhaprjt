
namespace BudgetManagement.Application.Common.Interfaces.IMiscMaster
{
    public interface IMiscMasterCommandRepository
    {
        Task<BudgetManagement.Domain.Entities.MiscMaster> CreateAsync(BudgetManagement.Domain.Entities.MiscMaster miscMaster);

        Task<int> GetMaxSortOrderAsync();

        Task<bool> UpdateAsync(int id, BudgetManagement.Domain.Entities.MiscMaster miscMaster);

        Task<bool> DeleteAsync(int id, BudgetManagement.Domain.Entities.MiscMaster miscMaster);  
        Task<Dictionary<int, Domain.Entities.MiscMaster>> GetManyByIdsAsync(IEnumerable<int> ids, CancellationToken ct);
    }
}