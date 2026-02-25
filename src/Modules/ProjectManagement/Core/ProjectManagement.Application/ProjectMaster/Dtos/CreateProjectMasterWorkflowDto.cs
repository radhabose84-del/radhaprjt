namespace ProjectManagement.Application.ProjectMaster.Command.CreateProjectMaster
{
    public class CreateProjectMasterWorkflowDto
    {
         public ProjectMasterWorkFlowDto Header { get; set; } = default!;
           public List<ProjectMasterWorkFlowDto> Lines { get; set; } = new();
    }
}