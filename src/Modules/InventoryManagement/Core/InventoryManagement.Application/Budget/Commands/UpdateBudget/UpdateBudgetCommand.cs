using MediatR;
using Contracts.Common;

namespace InventoryManagement.Application.Budget.Commands.UpdateBudget
{
    public class UpdateBudgetCommand : IRequest<bool>, IRequirePermission
    {
        public int BudgetId { get; set; }
        public decimal? YearBudgetAmount { get; set; }
        public List<UpdateBudgetDetailDto>? BudgetDetails { get; set; }        
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }

    public class UpdateBudgetDetailDto
    {
        public int DetailId { get; set; }
        public decimal NewAmount { get; set; }
        public string? Remarks { get; set; }
    }
}
