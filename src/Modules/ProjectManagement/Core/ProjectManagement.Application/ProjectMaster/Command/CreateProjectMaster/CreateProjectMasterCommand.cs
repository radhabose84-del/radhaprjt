using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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