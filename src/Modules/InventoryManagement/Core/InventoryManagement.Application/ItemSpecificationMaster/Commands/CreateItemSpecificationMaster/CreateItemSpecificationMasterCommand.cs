
using Contracts.Common;
using MediatR;

namespace InventoryManagement.Application.ItemSpecificationMaster.Commands.CreateItemSpecificationMaster
{
    public class CreateItemSpecificationMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public string? SpecificationCode { get; set; }
        public string? SpecificationName { get; set; }
        public int Order { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
