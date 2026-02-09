using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectManagement.Application.ProjectMaster.Queries.Dtos;
using ProjectManagement.Application.ProjectMaster.Queries.GetProjectMaster;
using MediatR;

namespace ProjectManagement.Application.ProjectMaster.Queries.GetProjectMasterById
{
    public class GetProjectMasterByIdQuery:  IRequest<GetProjectMasterDto>
    {
        public int Id { get; set; }
        
    }
}