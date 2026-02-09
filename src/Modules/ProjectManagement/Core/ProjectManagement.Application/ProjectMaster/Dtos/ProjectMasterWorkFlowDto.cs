using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagement.Application.ProjectMaster.Command.CreateProjectMaster
{
    public class ProjectMasterWorkFlowDto
    {
        public int Id { get; set; }      // Project Id (from entity.Id)
        public int ProjectTypeId { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
        public int StatusId { get; set; }     
        
         
    }
}