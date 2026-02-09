using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagement.Application.ProjectMaster.Command.CreateProjectMaster
{
    public class CreateProjectMasterWorkflowDto
    {
         public ProjectMasterWorkFlowDto Header { get; set; } = default!;
           public List<ProjectMasterWorkFlowDto> Lines { get; set; } = new();
    }
}