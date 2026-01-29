using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Domain.Entities.Workflow
{
    public class ApprovalStepUnitMapping
    {
        public int Id { get; set; }
        public int ApprovalStepDetailId { get; set; }
        public int UnitId { get; set; }
        public ApprovalStepDetail ApprovalStepDetail { get; set; }
    
    }
}