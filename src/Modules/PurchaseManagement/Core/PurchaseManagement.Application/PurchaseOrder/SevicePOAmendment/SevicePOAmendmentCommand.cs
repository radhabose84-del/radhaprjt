using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ServicePO;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.SevicePOAmendment
{
    public class SevicePOAmendmentCommand : IRequest<int>
    {
        public CreateServicePurchaseOrderDto  Dto  { get; set; } = default!;

          
    }
}
