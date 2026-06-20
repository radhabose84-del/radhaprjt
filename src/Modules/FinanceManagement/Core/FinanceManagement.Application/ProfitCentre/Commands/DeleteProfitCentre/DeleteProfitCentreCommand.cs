using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.ProfitCentre.Commands.DeleteProfitCentre
{
    public sealed record DeleteProfitCentreCommand(int Id) : IRequest<bool>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
