using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Workflow.ApprovalRules.Queries.GetAllApprovalRule
{
    public class ApprovalRuleDto
    {
        public int Id { get; set; }
        public int ActionId { get; set; }
        public string Action { get; set; }
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public DateOnly EffectiveTo { get; set; }
        public int Priority { get; set; }
        public byte IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string CreatedByName { get; set; }
        public int ModifiedBy { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
        public string ModifiedByName { get; set; }
        // public WorkflowTypeDto WorkflowType { get; set; }
    }
}