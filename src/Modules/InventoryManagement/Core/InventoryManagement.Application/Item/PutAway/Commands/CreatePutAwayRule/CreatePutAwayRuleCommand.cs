using MediatR;

namespace InventoryManagement.Application.Item.PutAway.Commands.CreatePutAwayRule
{
   public sealed record CreatePutAwayRuleCommand(CreatePutAwayRuleRequest Body) : IRequest<int>;
}