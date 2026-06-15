using ProjectManagement.Application.ProjectMaster.Queries.Dtos;
using MediatR;
using Contracts.Common;

namespace ProjectManagement.Application.ProjectMaster.Command.UpdateProjectMaster
{
    public class UpdateProjectMasterCommand: IRequest<ProjectMasterDto>, IRequirePermission
    {
      public UpdateProjectMasterDto Project { get; set; } = default!;
        
      public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
