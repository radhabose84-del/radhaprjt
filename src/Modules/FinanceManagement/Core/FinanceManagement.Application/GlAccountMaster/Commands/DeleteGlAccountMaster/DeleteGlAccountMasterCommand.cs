using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.GlAccountMaster.Commands.DeleteGlAccountMaster
{
    public sealed record DeleteGlAccountMasterCommand(int Id) : IRequest<bool>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
