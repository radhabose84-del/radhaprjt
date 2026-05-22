using MediatR;
using Contracts.Common;

namespace InventoryManagement.Application.Item.ItemGroup.Commands.UpdateItemGroup
{
    public class UpdateItemGroupCommand : IRequest<int>, IRequirePermission
    {
        public int Id { get; set; }
        public string? ItemGroupCode { get; set; }
        public string? ItemGroupName { get; set; }      
        public byte IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
