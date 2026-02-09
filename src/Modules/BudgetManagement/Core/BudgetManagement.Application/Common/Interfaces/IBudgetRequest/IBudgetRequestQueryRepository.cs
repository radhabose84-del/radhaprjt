using BudgetManagement.Application.BudgetRequest;
using BudgetManagement.Application.BudgetRequest.Queries.GetBudgetRequestPending;

namespace BudgetManagement.Application.Common.Interfaces.IBudgetRequest;

public interface IBudgetRequestQueryRepository
{
    Task<(List<BudgetRequestListItemDto> Items, int Total)> GetAllAsync(int? statusId, int pageNumber, int pageSize, string? searchTerm, CancellationToken ct = default);
    Task<BudgetRequestDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<string> GetBaseDirectoryAsync(CancellationToken ct = default);
    Task<(List<GetBudgetRequestPendingDto> Items, int Total)> GetPendingRequestAsync(int pageNumber, int pageSize, string? search, CancellationToken ct = default);   
    Task<bool> AllocationExistsAsync(int financialYearId, int requestById, int? requestMonthId, int? budgetGroupId, int? projectId, int? wbsId, CancellationToken ct = default);

}
