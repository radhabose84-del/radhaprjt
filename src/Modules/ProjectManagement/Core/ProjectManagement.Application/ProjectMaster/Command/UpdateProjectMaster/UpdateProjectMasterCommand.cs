using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectManagement.Application.ProjectMaster.Queries.Dtos;
using MediatR;

namespace ProjectManagement.Application.ProjectMaster.Command.UpdateProjectMaster
{
    public class UpdateProjectMasterCommand: IRequest<ProjectMasterDto>
    {
      public UpdateProjectMasterDto Project { get; set; } = default!;
        
    }
}