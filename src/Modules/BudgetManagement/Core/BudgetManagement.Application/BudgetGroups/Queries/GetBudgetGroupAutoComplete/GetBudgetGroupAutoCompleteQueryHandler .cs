using BudgetManagement.Application.Common.Interfaces.IBudgetGroupMaster;
using MediatR;

namespace BudgetManagement.Application.BudgetGroups.Queries.GetBudgetGroupAutoComplete
{
    public class GetBudgetGroupAutoCompleteQueryHandler : IRequestHandler<GetBudgetGroupAutoCompleteQuery, List<BudgetGroupAutoCompleteDto>>
    {
        private readonly IBudgetGroupQueryRepository _budgetGroupQueryRepository;

        public GetBudgetGroupAutoCompleteQueryHandler(
            IBudgetGroupQueryRepository budgetGroupQueryRepository)
        {
            _budgetGroupQueryRepository = budgetGroupQueryRepository;
        }

        public async Task<List<BudgetGroupAutoCompleteDto>> Handle(GetBudgetGroupAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var search = request.SearchPattern ?? string.Empty;
            // Repo already returns BudgetGroupAutoCompleteDto list
            var result = await _budgetGroupQueryRepository.GetBudgetGroupAutoCompleteAsync(
                search,
                request.EmergencyPoApplicable,
                cancellationToken);

            return result;
        }
    }
}
