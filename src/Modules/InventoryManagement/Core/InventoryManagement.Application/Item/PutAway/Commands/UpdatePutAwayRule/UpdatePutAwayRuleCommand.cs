using InventoryManagement.Application.Item.PutAway.Commands.CreatePutAwayRule;
using MediatR;

namespace InventoryManagement.Application.Item.PutAway.Commands.UpdatePutAwayRule
{
    public sealed record UpdatePutAwayRuleCommand(int Id, CreatePutAwayRuleRequest Body) : IRequest<Unit>;
}