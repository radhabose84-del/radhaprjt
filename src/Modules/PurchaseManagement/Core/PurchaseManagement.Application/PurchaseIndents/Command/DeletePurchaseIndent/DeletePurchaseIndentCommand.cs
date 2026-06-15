using MediatR;
using Contracts.Common;

namespace PurchaseManagement.Application.PurchaseIndents.Command.DeletePurchaseIndent
{
    public class DeletePurchaseIndentCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
