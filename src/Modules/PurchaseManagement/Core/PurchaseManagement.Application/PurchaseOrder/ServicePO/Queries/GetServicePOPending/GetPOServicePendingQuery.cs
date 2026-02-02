using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.GetServicePOPending
{
    public class GetPOServicePendingQuery  : IRequest<(List<GetServicePOPendingGroupDto> Items, int TotalCount)>
    {
        public int? PageNumber { get; set; } = 1;
        public int? PageSize   { get; set; } = 15;
        public string? SearchTerm { get; set; }
        public int? PoId { get; set; }
    
        
    }
}