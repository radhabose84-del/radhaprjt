using Contracts.Common;
using InventoryManagement.Application.Item.PutAway.Queries.GetAllPutAwayRule;
using MediatR;

namespace InventoryManagement.Application.Item.PutAway.Queries.GetPutAwayRules
{
    public class GetPutAwayRulesQuery : IRequest<ApiResponseDTO<List<PutAwayRuleListDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize  { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}
