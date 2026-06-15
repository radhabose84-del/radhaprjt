using MediatR;
using Contracts.Common;

namespace PurchaseManagement.Application.PaymentTermMaster.Command.DeletePaymentTermMaster
{
    public class DeletePaymentTermMasterCommand: IRequest<bool>, IRequirePermission
    {
        public int Id { get; init; }
        
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
