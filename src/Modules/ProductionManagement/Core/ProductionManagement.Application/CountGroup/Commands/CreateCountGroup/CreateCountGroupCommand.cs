using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.CountGroup.Commands.CreateCountGroup
{
    public class CreateCountGroupCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public string? CountGroupCode { get; set; }
        public string? CountGroupName { get; set; }
        public string? Description { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
