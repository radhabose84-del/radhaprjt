using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.Budget;
using InventoryManagement.Domain.Entities.Budget;
using MediatR;

namespace InventoryManagement.Application.Budget.Commands.UpdateBudget
{
    public class UpdateBudgetCommandHandler : IRequestHandler<UpdateBudgetCommand, bool>
    {
        private readonly IBudgetCommandRepository _budgetRepository;

        public UpdateBudgetCommandHandler(IBudgetCommandRepository budgetRepository)
        {
            _budgetRepository = budgetRepository;
        }

        public async Task<bool> Handle(UpdateBudgetCommand request, CancellationToken cancellationToken)
        {
            // ✅ Update Master
            if (request.YearBudgetAmount.HasValue)
            {
                await _budgetRepository.UpdateBudgetMasterAsync(
                    request.BudgetId,
                    request.YearBudgetAmount.Value,
                    "MasterUpdation"                  
                );
            }

            // ✅ Update Details
            if (request.BudgetDetails != null && request.BudgetDetails.Any())
            {
                foreach (var detail in request.BudgetDetails)
                {
                    await _budgetRepository.UpdateBudgetDetailAsync(
                        detail.DetailId,
                        detail.NewAmount,
                        detail.Remarks ?? "Budget updated"                    
                    );
                }
            }

            return true;
        }
    }
}
