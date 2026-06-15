
using Contracts.Common;
using MediatR;

namespace InventoryManagement.Application.ItemSpecificationValue.Commands.UpdateItemSpecificationValue
{
    public class UpdateItemSpecificationValueCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public int SpecificationMasterId { get; set; }
        public string? SpecificationValue { get; set; }
        public int IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
