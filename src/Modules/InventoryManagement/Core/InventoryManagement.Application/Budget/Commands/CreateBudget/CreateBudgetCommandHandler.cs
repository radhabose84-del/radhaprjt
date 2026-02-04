using MediatR;
using InventoryManagement.Domain.Entities.Budget;
using InventoryManagement.Application.Budget.Commands.CreateBudget;
using InventoryManagement.Application.Common.Interfaces.Budget;
using AutoMapper;

namespace InventoryManagement.Application.Features.Budget.Commands.CreateBudget
{
    public class CreateBudgetCommandHandler : IRequestHandler<CreateBudgetCommand, int>
    {
        private readonly IBudgetCommandRepository _budgetRepository;
        private readonly IMapper _mapper;

        public CreateBudgetCommandHandler(IBudgetCommandRepository budgetRepository, IMapper mapper)
        {
            _budgetRepository = budgetRepository;
            _mapper = mapper;
        }
        

        public async Task<int> Handle(CreateBudgetCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<BudgetMaster>(request);
            var result = await _budgetRepository.CreateBudgetAsync(entity);
            return result;
        }
    }
}
