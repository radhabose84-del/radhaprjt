using InventoryManagement.Application.Common.Interfaces.Budget;
using MediatR;

namespace InventoryManagement.Application.Budget.Queries.GetBudgetLogs
{
    public class GetBudgetLogsQueryHandler : IRequestHandler<GetBudgetLogsQuery, List<BudgetLogDto>>
    {
        private readonly IBudgetLogQueryRepository _repository;

        public GetBudgetLogsQueryHandler(IBudgetLogQueryRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<BudgetLogDto>> Handle(GetBudgetLogsQuery request, CancellationToken cancellationToken)
        {
            var logs = await _repository.GetLogsAsync(request.BudgetId, request.BudgetDetailId);
            return logs;
        }
    }
}
