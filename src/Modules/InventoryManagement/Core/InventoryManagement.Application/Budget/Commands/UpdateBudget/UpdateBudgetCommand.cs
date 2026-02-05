using MediatR;

namespace InventoryManagement.Application.Budget.Commands.UpdateBudget
{
    public class UpdateBudgetCommand : IRequest<bool>
    {
        public int BudgetId { get; set; }
        public decimal? YearBudgetAmount { get; set; }
        public List<UpdateBudgetDetailDto>? BudgetDetails { get; set; }        
    }

    public class UpdateBudgetDetailDto
    {
        public int DetailId { get; set; }
        public decimal NewAmount { get; set; }
        public string? Remarks { get; set; }
    }
}
