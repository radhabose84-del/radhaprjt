
using Contracts.Common;
using MediatR;

namespace InventoryManagement.Application.ItemSpecificationMaster.Commands.UpdateItemSpecificationMaster
{
    public class UpdateItemSpecificationMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public string? SpecificationName { get; set; }
        public int Order { get; set; }
        public int IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
