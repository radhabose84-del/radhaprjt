using InventoryManagement.Application.Common.Interfaces.Budget;
using MediatR;

namespace InventoryManagement.Application.Budget.Queries.GetBudgetById
{
    public class GetBudgetByIdQueryHandler : IRequestHandler<GetBudgetByIdQuery, BudgetResponseDto>
    {
        private readonly IBudgetQueryRepository _repository;

        public GetBudgetByIdQueryHandler(IBudgetQueryRepository repository)
        {
            _repository = repository;
        }

        public async Task<BudgetResponseDto> Handle(GetBudgetByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetBudgetByIdAsync(request.BudgetId);
            if (result == null)
                throw new KeyNotFoundException($"Item Group with Id {request.BudgetId} not found.");

            return result;
        }
    }
}
