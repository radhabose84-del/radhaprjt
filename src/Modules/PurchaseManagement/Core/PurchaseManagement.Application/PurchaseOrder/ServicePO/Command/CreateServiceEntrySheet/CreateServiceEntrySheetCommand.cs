using MediatR;
using Contracts.Common;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.CreateServiceEntrySheet
{
    public class CreateServiceEntrySheetCommand : IRequest<int>, IRequirePermission
    {
      
         public CreateServiceSheetDto CreateServiceSheet { get; set; } = null!;
         public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
