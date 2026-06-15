using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.DiscountMaster.Commands.DeleteDiscountMaster
{
    public sealed record DeleteDiscountMasterCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
