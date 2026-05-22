using InventoryManagement.Application.Item.PutAway.Commands.CreatePutAwayRule;
using Contracts.Common;
using MediatR;

namespace InventoryManagement.Application.Item.PutAway.Commands.UpdatePutAwayRule
{
    public sealed record UpdatePutAwayRuleCommand(int Id, CreatePutAwayRuleRequest Body) : IRequest<Unit>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanUpdate;
}
}