using MediatR;
using Contracts.Common;

namespace PurchaseManagement.Application.ServiceMaster.Commands.DeleteService
{
    public class DeleteServiceCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }
        
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
