
using MediatR;
using Contracts.Common;

namespace InventoryManagement.Application.Item.ItemGroup.Commands.CreateItemGroup
{
    public class CreateItemGroupCommand : IRequest<int>, IRequirePermission
    {        
        public string? ItemGroupCode { get; set; }
        public string? ItemGroupName { get; set; }  
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
