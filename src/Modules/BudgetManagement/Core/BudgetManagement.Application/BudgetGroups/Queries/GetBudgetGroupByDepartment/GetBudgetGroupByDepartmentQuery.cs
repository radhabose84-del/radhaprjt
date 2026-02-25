using MediatR;

namespace BudgetManagement.Application.BudgetGroups.Queries.GetBudgetGroupByDepartment
{
    public record GetBudgetGroupByDepartmentQuery(
        int DepartmentId,
        string? SearchPattern
    ) : IRequest<List<BudgetGroupAutoCompleteDto>>;
}
