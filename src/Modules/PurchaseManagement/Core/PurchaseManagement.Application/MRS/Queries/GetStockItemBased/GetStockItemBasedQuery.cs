using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.MRS.Queries.GetStockItemBased
{
    public class GetStockItemBasedQuery : IRequest<List<GetStockItemDto>>
    {
        public int ItemId { get; set; }
        public int WarehouseId { get; set; }
      
    }
}