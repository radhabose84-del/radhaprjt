using MediatR;
using Contracts.Common;

namespace InventoryManagement.Application.Item.ItemGroup.Commands.DeleteItemGroup
{
    public class DeleteItemGroupCommand : IRequest<int>, IRequirePermission 
    {
        public int Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
