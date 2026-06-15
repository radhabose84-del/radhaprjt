using MediatR;
using Contracts.Common;

namespace BudgetManagement.Application.BudgetRequest.Commands.Delete;
public class DeleteBudgetRequestCommand : IRequest, IRequirePermission
{
    public int Id { get; set; }
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
