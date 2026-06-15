using MediatR;
using Contracts.Common;

namespace InventoryManagement.Application.Item.PutAway.Commands.DeletePutAwayRule
{    
    public class DeletePutAwayRuleCommand : IRequest<int>, IRequirePermission 
    {
        public int Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
