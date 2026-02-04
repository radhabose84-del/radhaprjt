using InventoryManagement.Application.Item.PutAway.Queries.GetAllPutAwayRule;
using MediatR;

namespace InventoryManagement.Application.Item.PutAway.Queries.GetPutAwayRuleById
{
    public class GetPutAwayRuleByIdQuery : IRequest<PutAwayRuleDetailDto>
    {
        public int Id { get; set; }
    }
}
