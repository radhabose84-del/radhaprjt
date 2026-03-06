using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Domain.Entities.Workflow
{
    public class ApprovalStepDepartmentMapping
    {
        public int Id { get; set; }
        public int ApprovalStepDetailId { get; set; }
        public int DepartmentId { get; set; }
        public ApprovalStepDetail ApprovalStepDetail { get; set; }
    }
}