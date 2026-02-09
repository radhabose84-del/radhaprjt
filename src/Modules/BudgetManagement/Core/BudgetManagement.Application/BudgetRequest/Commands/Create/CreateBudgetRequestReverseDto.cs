
namespace BudgetManagement.Application.BudgetRequest.Commands.Create
{
    public class CreateBudgetRequestReverseDto
    {
        public BudgetRequestWorkFlowDto? Header { get; set; }
        public ICollection<BudgetRequestWorkFlowDto>? Lines { get; set; }
    }
    
}