using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.CreateServiceEntrySheet
{
    public class CreateServiceEntrySheetCommand : IRequest<int>
    {
      
         public CreateServiceSheetDto CreateServiceSheet { get; set; } = null!;
    }
}