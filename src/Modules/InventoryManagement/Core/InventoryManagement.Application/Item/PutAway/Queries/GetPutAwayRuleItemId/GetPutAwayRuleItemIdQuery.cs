using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace InventoryManagement.Application.Item.PutAway.Queries.GetPutAwayRuleItemId
{
    public class GetPutAwayRuleItemIdQuery : IRequest<List<GetPutAwayRuleItemIdDto>>
    {
        public List<int>? ItemIds { get; set; } // For multi-select filter
        public List<int>? WarehouseIds { get; set; } // For multi-select filter
    }
}