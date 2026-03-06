using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Workflow.ApprovalStepDetails.Commands.UpdateApprovalStepDetail
{
    public class ApprovalStepDepartmentMappingUpdateDto
    {
        public int ApprovalStepDetailId { get; set; }
        public int DepartmentId { get; set; }
    }
}