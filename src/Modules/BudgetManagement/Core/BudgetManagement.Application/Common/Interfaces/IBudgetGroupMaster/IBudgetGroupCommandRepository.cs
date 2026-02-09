using System.Threading;
using System.Threading.Tasks;

namespace BudgetManagement.Application.Common.Interfaces.IBudgetGroupMaster
{
    public interface IBudgetGroupCommandRepository
    {
        Task<int> CreateAsync(BudgetManagement.Domain.Entities.BudgetGroup entity, CancellationToken ct = default);
        Task<int> UpdateAsync(int id, BudgetManagement.Domain.Entities.BudgetGroup entity, CancellationToken ct = default);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default);
        Task<BudgetManagement.Domain.Entities.BudgetGroup?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<bool> ExistsByNameAndUnitDepartmentAsync(string name, int unitId, int departmentId, CancellationToken ct = default);
        Task<bool> IsNameDuplicateAsync(string name, int excludeId,int unitId, int departmentId, CancellationToken ct = default);
    }
}
