using ProjectManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using ProjectManagement.Application.ProjectMaster.Queries.Dtos;
using MediatR;
using Contracts.Common;

namespace ProjectManagement.Application.ProjectMaster.Command.CreateProjectMaster
{
    public class CreateProjectMasterCommand   : IRequest<ProjectMasterDto>, IRequirePermission   
     {
        public CreateProjectMasterDto Project { get; set; } = default!;
    

       
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
