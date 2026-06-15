using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.AccountTypeMaster.Commands.DeleteAccountTypeMaster
{
    public sealed record DeleteAccountTypeMasterCommand(int Id) : IRequest<bool>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
