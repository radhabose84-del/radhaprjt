using MediatR;
using Contracts.Common;


namespace BudgetManagement.Application.BudgetGroup.Command.DeleteBudgetGroup
{
    public class DeleteBudgetGroupCommand : IRequest<int>, IRequirePermission
    {
        public int Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
