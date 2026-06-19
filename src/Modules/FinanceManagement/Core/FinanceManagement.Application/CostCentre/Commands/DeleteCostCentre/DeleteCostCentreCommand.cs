using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.CostCentre.Commands.DeleteCostCentre
{
    public sealed record DeleteCostCentreCommand(int Id) : IRequest<bool>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
