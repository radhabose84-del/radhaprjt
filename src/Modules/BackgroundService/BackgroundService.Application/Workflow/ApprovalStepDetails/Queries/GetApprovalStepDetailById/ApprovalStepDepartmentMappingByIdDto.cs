using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Workflow.ApprovalStepDetails.Queries.GetApprovalStepDetailById
{
    public class ApprovalStepDepartmentMappingByIdDto
    {
        public int Id { get; set; }
        public int DepartmentId { get; set; }
    }
}