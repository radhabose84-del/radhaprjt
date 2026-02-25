using ProjectManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using ProjectManagement.Application.ProjectMaster.Queries.Dtos;
using MediatR;

namespace ProjectManagement.Application.ProjectMaster.Command.CreateProjectMaster
{
    public class CreateProjectMasterCommand   : IRequest<ProjectMasterDto>   
     {
        public CreateProjectMasterDto Project { get; set; } = default!;
    

       
    }
}