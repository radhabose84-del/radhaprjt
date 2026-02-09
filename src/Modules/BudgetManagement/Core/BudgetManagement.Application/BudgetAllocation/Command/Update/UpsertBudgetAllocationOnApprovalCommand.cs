using MediatR;

namespace BudgetManagement.Application.BudgetAllocation.Command.Update
{
    public sealed class UpsertBudgetAllocationOnApprovalCommand : IRequest<bool>
    {
        public int BudgetRequestId { get; set; }   
    }
}
