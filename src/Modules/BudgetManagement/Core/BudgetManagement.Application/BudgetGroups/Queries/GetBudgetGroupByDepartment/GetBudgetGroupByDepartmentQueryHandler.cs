using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BudgetManagement.Application.Common.Interfaces.IBudgetGroupMaster;
using MediatR;

namespace BudgetManagement.Application.BudgetGroups.Queries.GetBudgetGroupByDepartment
{
    public class GetBudgetGroupByDepartmentQueryHandler
        : IRequestHandler<GetBudgetGroupByDepartmentQuery, List<BudgetGroupAutoCompleteDto>>
    {
        private readonly IBudgetGroupQueryRepository _repo;

        public GetBudgetGroupByDepartmentQueryHandler(IBudgetGroupQueryRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<BudgetGroupAutoCompleteDto>> Handle(
            GetBudgetGroupByDepartmentQuery request,
            CancellationToken ct)
        {
            return await _repo.GetBudgetGroupByDepartmentAsync(
                request.DepartmentId,
                request.SearchPattern,
                ct);
        }
    }
}
