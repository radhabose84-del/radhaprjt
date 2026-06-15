using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.ProcessMaster.Commands.CreateProcessMaster
{
    public class CreateProcessMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public string? ProcessName { get; set; }
        public bool CombingRequired { get; set; }
        public string? Description { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
