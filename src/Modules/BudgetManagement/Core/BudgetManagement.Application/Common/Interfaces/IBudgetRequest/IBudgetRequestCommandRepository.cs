
using BudgetManagement.Application.BudgetRequest.Commands.Create;

namespace BudgetManagement.Application.Common.Interfaces.IBudgetRequest;

public interface IBudgetRequestCommandRepository
{
    Task<Domain.Entities.BudgetRequest> AddAsync(Domain.Entities.BudgetRequest entity, CancellationToken ct = default);
    Task UpdateAsync(Domain.Entities.BudgetRequest entity, CancellationToken ct = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default);
    Task<Domain.Entities.BudgetRequest?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<string> GenerateCodeAsync(int unitId, DateOnly requestDate, CancellationToken ct = default);
    Task<bool> RemoveImageReferenceAsync(string imagePath);
    Task<bool> UpdateImageAsync(int requestId, string imageName, CancellationToken ct = default);
    Task<bool> UpdateRequestApproveAsync(int id, int statusId, CancellationToken ct = default);
    Task<bool> RollbackStatusAsync(int id, CancellationToken ct = default);
    Task<BudgetRequestWorkFlowDto> GetByIdBudgetRequestWorkFlowAsync(int id);
    Task<bool> RollbackRequestStatusAsync(int id, CancellationToken ct = default);
    Task<bool> ExistsOpexAsync(int unitId, int financialYearId, int requestTypeId, int budgetGroupId,
         int? requestById, CancellationToken ct);

    Task<bool> ExistsCapexAsync(int unitId, int financialYearId, int requestTypeId, int projectId,int wbsId, int? requestById, CancellationToken ct);
     Task<bool> ExistsOpexForUpdateAsync(
        int excludeId,
        int unitId,
        int requestTypeId,
        int budgetGroupId,
        DateOnly fromDate,
        DateOnly toDate,
        int? requestById,
        CancellationToken ct);

    Task<bool> ExistsCapexForUpdateAsync(
        int excludeId,
        int unitId,
        int requestTypeId,
        int projectId,int wbsId,
        DateOnly fromDate,
        DateOnly toDate,
        int? requestById,
        CancellationToken ct);
}
