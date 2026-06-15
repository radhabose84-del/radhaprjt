using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.CreateServiceEntrySheet;
using MediatR;
using Contracts.Common;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.UpdateServiceEntrySheet
{
    public class UpdateServiceEntrySheetCommand: IRequest<int>, IRequirePermission   {
        
        public int Id { get; set; }
        public CreateServiceSheetDto Data { get; set; } = null!;
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
