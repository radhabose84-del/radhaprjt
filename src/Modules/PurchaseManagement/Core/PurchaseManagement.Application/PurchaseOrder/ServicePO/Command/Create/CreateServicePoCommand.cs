using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ServicePO;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.Create
{
    public class CreateServicePoCommand : IRequest<int>
    {
        public  required CreateServicePurchaseOrderDto Data { get; init; } 

        
    }


    
}