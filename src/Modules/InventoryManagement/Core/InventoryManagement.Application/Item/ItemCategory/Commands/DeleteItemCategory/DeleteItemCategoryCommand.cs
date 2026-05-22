using MediatR;
using Contracts.Common;

namespace InventoryManagement.Application.Item.ItemCategory.Commands.DeleteItemCategory
{
    public class DeleteItemCategoryCommand : IRequest<int>, IRequirePermission 
    {
        public int Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
