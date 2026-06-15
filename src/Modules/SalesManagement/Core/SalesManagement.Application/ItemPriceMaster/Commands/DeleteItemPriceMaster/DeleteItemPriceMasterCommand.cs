using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.ItemPriceMaster.Commands.DeleteItemPriceMaster;

public sealed record DeleteItemPriceMasterCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
