using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ServicePO;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.GetServicePO
{
    
    
    public class GetServicePOByIdQuery : IRequest<PurchaseOrderServiceDetailDto?>
    {


        public int Id { get; set; }
    public GetServicePOByIdQuery(int id) => Id = id;
    }
}