using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.VoucherType.Commands.DeleteVoucherType
{
    public sealed record DeleteVoucherTypeCommand(int Id) : IRequest<bool>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
