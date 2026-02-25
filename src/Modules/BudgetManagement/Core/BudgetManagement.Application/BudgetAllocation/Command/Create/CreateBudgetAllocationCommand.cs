using MediatR;

namespace BudgetManagement.Application.BudgetAllocation.Command.Create
{
    public class CreateBudgetAllocationCommand : IRequest<int>  
    {
        public List<CreateBudgetAllocationDto> createBudgetAllocations { get; set; } = new();
    }
}