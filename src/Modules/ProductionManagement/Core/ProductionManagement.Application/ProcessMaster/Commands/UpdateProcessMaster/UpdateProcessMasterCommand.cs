using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.ProcessMaster.Commands.UpdateProcessMaster
{
    public class UpdateProcessMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public string? ProcessName { get; set; }
        public bool CombingRequired { get; set; }
        public string? Description { get; set; }
        public int IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
