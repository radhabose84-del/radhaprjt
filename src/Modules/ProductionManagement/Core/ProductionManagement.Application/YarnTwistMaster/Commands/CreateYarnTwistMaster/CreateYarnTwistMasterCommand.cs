using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.YarnTwistMaster.Commands.CreateYarnTwistMaster
{
    public class CreateYarnTwistMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public string? TwistName { get; set; }
        public string? Description { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
