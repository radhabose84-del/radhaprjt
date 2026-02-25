using PurchaseManagement.Application.PurchaseOrder.Dtos.ServicePO;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.Update
{
    public class UpdateServicePoCommand : IRequest<bool>
    {
     //   public int Id { get; init; }              // PO Id to update
        public required  CreateServicePurchaseOrderDto  Data { get; init; }
      //  public required ServicePurchaseOrderUpdateDto Data { get; init; }

    }
}