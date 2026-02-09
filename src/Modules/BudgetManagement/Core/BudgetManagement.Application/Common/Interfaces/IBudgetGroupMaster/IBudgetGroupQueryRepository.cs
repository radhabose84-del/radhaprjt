using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetManagement.Domain.Entities;
using System.Threading;
using BudgetManagement.Application.BudgetGroups;

namespace BudgetManagement.Application.Common.Interfaces.IBudgetGroupMaster
{
    public interface IBudgetGroupQueryRepository
    {
        Task<(List<BudgetGroupListItemDto> Items, int TotalCount)> GetAllAsync(BudgetGroupListFilterDto filter, CancellationToken ct = default);
        Task<BudgetGroupDto?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<BudgetGroupAutoCompleteDto>> GetBudgetGroupAutoCompleteAsync(string searchPattern, CancellationToken ct = default);
        Task<bool> SoftDeleteValidation(int id, CancellationToken ct = default);
        Task<List<BudgetGroupAutoCompleteDto>> GetBudgetGroupByDepartmentAsync(int departmentId, string? searchPattern, CancellationToken ct = default);
    }
}