using MediatR;
using Contracts.Common;

namespace BudgetManagement.Application.BudgetAllocation.Command.Create
{
    public class CreateBudgetAllocationCommand : IRequest<int>, IRequirePermission  
    {
        public List<CreateBudgetAllocationDto> createBudgetAllocations { get; set; } = new();
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
