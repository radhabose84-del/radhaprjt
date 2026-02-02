using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetServicePOLines
{
    public class GetServicePOLinesQuery : IRequest<IReadOnlyList<GetServicePOLinesDto>>
    {
       public int POId { get; set; }
    }
}