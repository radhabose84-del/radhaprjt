using MediatR;

namespace InventoryManagement.Application.Budget.Commands.CreateBudget
{
    public class CreateBudgetCommand : IRequest<int>
    {        
        public int BudgetGroupId { get; set; }
        public int FiscalYear { get; set; }
        public decimal YearBudgetAmount { get; set; }
        public byte? Is_MRApplicable { get; set; }
        public byte? Is_POApplicable { get; set; }
        public byte? Is_ServiceApplicable { get; set; }
        public List<BudgetDetailDto> BudgetDetails { get; set; } = new();        
    }

    public class BudgetDetailDto
    {
        public int Month { get; set; }
        public decimal BudgetAmount { get; set; }
    }
}
