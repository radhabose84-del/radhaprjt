using PurchaseManagement.Application.PurchaseOrder.Dtos.ServicePO;
using MediatR;
using Contracts.Common;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.Create
{
    public class CreateServicePoCommand : IRequest<int>, IRequirePermission
    {
        public  required CreateServicePurchaseOrderDto Data { get; init; } 

        
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }


    
}
