using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.DispatchAddressMapping.Commands.UpdateDispatchAddressMapping
{
    public class UpdateDispatchAddressMappingCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public bool IsDefault { get; set; }
        public int IsActive { get; set; }  // 1=Active, 0=Inactive
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
