using InventoryManagement.Application.Common.Interfaces.Budget;
using MediatR;

namespace InventoryManagement.Application.Budget.Queries.GetAllBudgets
{
    public class GetAllBudgetsQueryHandler : IRequestHandler<GetAllBudgetsQuery, List<BudgetListDto>>
    {
        private readonly IBudgetQueryRepository _repository;

        public GetAllBudgetsQueryHandler(IBudgetQueryRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<BudgetListDto>> Handle(GetAllBudgetsQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetAllBudgetsAsync(request.FiscalYear);
        }
    }
}
