using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Workflow.ApprovalStepDetails.Queries.GetAllApprovalStepDetail
{
    public class ApprovalStepDetailDto
    {
        public int Id { get; set; }
        public int WorkFlowTypeId { get; set; }
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public int StepOrder { get; set; }
        public string TargetType { get; set; }
        public string TargetValue { get; set; }
        public string ApprovalStepName { get; set; }
        public byte StopOnFirstMatch { get; set; }
         public  byte IsEdit { get; set; }
        public int? TransactionTypeId { get; set; }
        public string? TransactionTypeName { get; set; }
        public byte IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string CreatedByName { get; set; }
        public int ModifiedBy { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
        public string ModifiedByName { get; set; }
        public WorkflowTypeApprovalStepDto WorkflowType { get; set; }
        public ApprovalStepDto ApprovalStep { get; set; }


    }
}