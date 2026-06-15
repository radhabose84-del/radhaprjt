
using Contracts.Common;
using MediatR;

namespace InventoryManagement.Application.ItemSpecificationValue.Commands.CreateItemSpecificationValue
{
    public class CreateItemSpecificationValueCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int SpecificationMasterId { get; set; }
        public string? SpecificationValue { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
