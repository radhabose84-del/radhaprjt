namespace ProjectManagement.Application.ProjectMaster.Command.CreateProjectMaster
{
    public class ProjectMasterActivityWorkFlowDto
    {
         public int ProjectId { get; set; }
        public int ActivityId { get; set; }       
        public int ApproverUserId { get; set; }    
        public int SequenceNo { get; set; }        
        public int StatusId { get; set; }          
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
    }
}