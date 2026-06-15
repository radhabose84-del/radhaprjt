using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.DispatchAddressMapping.Commands.CreateDispatchAddressMapping
{
    public class CreateDispatchAddressMappingCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int PartyId { get; set; }
        public int DispatchAddressId { get; set; }
        public int UsageTypeId { get; set; }
        public bool IsDefault { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
