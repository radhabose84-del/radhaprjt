
using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.BusinessUnit.Commands.UpdateBusinessUnit
{
    public class UpdateBusinessUnitCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public string? BusinessUnitName { get; set; }
        public string? Description { get; set; }
        public int IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
